class_name PlayMode
extends RefCounted

var mgr : GameManager
var player_coord : Vector2
var enemy_coords = []

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
	var player_tile = mgr.get_tile(player_coord)
	var goal_tile =  mgr.get_adjacent_tile(player_tile, dir)
	if goal_tile and goal_tile.tile_type == FloorTile.TileType.EMPTY\
			and player_tile.can_move(dir):
		goal_tile.set_tile_type(FloorTile.TileType.PLAYER)
		player_tile.set_tile_type(FloorTile.TileType.EMPTY)
		player_coord = goal_tile.coord
		goal_tile.set_selected(true)
		player_tile.set_selected(false)
