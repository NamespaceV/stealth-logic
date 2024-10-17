class_name PlayMode
extends RefCounted

var mgr : GameManager
var player_coord : Vector2
var enemy_coords = []
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
					enemy_coords.push_back(coord)
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
	for enemy_idx in enemy_coords.size():
		var enemy_tile = mgr.get_tile(enemy_coords[enemy_idx])
		for dir in 4:
			if not enemy_tile.can_move(dir):
				continue
			var adjacent_tile = mgr.get_adjacent_tile(enemy_tile, dir)
			if not adjacent_tile:
				continue
			if adjacent_tile.tile_type == FloorTile.TileType.PLAYER:
				player_lost = true
				enemy_tile.set_tile_type(FloorTile.TileType.EMPTY)
				adjacent_tile.set_tile_type(FloorTile.TileType.ENEMY)
				enemy_coords[enemy_idx] = adjacent_tile.coord
				return
			if adjacent_tile.tile_type == FloorTile.TileType.EMPTY:
				var distance = find_player(adjacent_tile, dir)
				if distance:
					enemy_tile.set_tile_type(FloorTile.TileType.EMPTY)
					adjacent_tile.set_tile_type(FloorTile.TileType.ENEMY)
					enemy_coords[enemy_idx] = adjacent_tile.coord


func find_player(tile : FloorTile, dir: GameManager.Dir):
	var distance = 0
	while tile and tile.can_move(dir):
		distance += 1
		tile = mgr.get_adjacent_tile(tile, dir)
		if tile and tile.tile_type == FloorTile.TileType.PLAYER:
			return distance
	return null
