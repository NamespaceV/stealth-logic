class_name PlayMode
extends RefCounted

var mgr : GameManager
var player_coord : Vector2
var enemy_states : Array[EnemyState] = []
var player_lost := false

func _init(manager:GameManager):
	mgr = manager
	var size = mgr.get_grid_size()

	for x in size.x:
		for y in size.y:
			var coord = Vector2(x,y)
			match (mgr.get_tile(coord).tile_type):
				FloorTile.TileType.EMPTY:
					pass
				FloorTile.TileType.PLAYER:
					if player_coord:
						push_warning("multiple players not supported", player_coord, coord)
					player_coord = coord
				FloorTile.TileType.ENEMY:
					enemy_states.push_back(EnemyState.new(coord, mgr, self))
	var player_tile = mgr.get_tile(player_coord)
	player_tile.set_selected(true)

func on_input(dir: GameManager.Dir):
	if player_lost: return
	var player_tile = mgr.get_tile(player_coord)
	var goal_tile =  mgr.get_adjacent_tile(player_tile, dir)
	if goal_tile and goal_tile.tile_type == FloorTile.TileType.EMPTY\
			and player_tile.can_move(dir):
		goal_tile.set_tile_type(FloorTile.TileType.PLAYER)
		player_tile.set_tile_type(FloorTile.TileType.EMPTY)
		player_coord = goal_tile.coord
		goal_tile.set_selected(true)
		player_tile.set_selected(false)
		move_enemies()


func move_enemies():
	for enemy in enemy_states:
		if player_lost: return
		enemy.move()

class EnemyState extends RefCounted:
	var mgr : GameManager
	var play_mode : PlayMode
	var coord : Vector2

	var last_seen_active := false
	var last_seen_current_turn := false
	var last_seen_location : Vector2
	var last_seen_dir : GameManager.Dir
	var last_seen_distance : int

	func _init(enemy_coord : Vector2, game_manager : GameManager, mode_state:PlayMode):
		coord = enemy_coord
		mgr = game_manager
		play_mode = mode_state

	func move():
		var enemy_tile = get_tile()
		last_seen_current_turn = false
		for dir in 4:
			if not enemy_tile.can_move(dir):
				continue
			var adjacent_tile = mgr.get_adjacent_tile(enemy_tile, dir)
			if not adjacent_tile:
				continue
			if adjacent_tile.tile_type == FloorTile.TileType.PLAYER:
				play_mode.player_lost = true
				move_to(adjacent_tile)
				return
			if adjacent_tile.tile_type == FloorTile.TileType.EMPTY:
				find_player(adjacent_tile, dir)

		if last_seen_active:
			move_to(mgr.get_adjacent_tile(get_tile(), last_seen_dir))
			last_seen_distance -= 1
			if last_seen_distance == 0:
				last_seen_active = false


	func find_player(tile : FloorTile, dir: GameManager.Dir):
		var distance = 1
		while tile and tile.can_move(dir):
			distance += 1
			tile = mgr.get_adjacent_tile(tile, dir)
			if tile and tile.tile_type == FloorTile.TileType.PLAYER:
				last_seen_active = true
				last_seen_current_turn = true
				last_seen_dir = dir
				last_seen_distance = distance

	func get_tile():
		return mgr.get_tile(coord)

	func move_to(new_tile):
		get_tile().set_tile_type(FloorTile.TileType.EMPTY)
		new_tile.set_tile_type(FloorTile.TileType.ENEMY)
		coord = new_tile.coord
