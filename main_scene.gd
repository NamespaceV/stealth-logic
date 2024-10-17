class_name GameManager
extends Node2D

var tile_scene = load("res://Interactables/FloorTile/floor_tile.tscn") as PackedScene

var grid = []
var selected_tile = null

enum Dir {
	RIGHT,
	DOWN,
	LEFT,
	UP,
}

var DIR_TO_OFFSET = [
	Vector2(1,0),
	Vector2(0,1),
	Vector2(-1,0),
	Vector2(0,-1),
]

var play_mode: PlayMode

var last_mouse_position

func dir_to_offset(dir:Dir) -> Vector2:
	return DIR_TO_OFFSET[dir]

func opposite_dir(dir:Dir) -> Dir:
	return (dir+2) % 4 as Dir

func get_tile(coord:Vector2) -> FloorTile:
	if coord.x < 0 or coord.y < 0:
		return null
	if coord.x >= grid.size():
		return null
	if coord.y >= grid[coord.x].size():
		return null
	return grid[coord.x][coord.y]

func get_grid_size() ->  Vector2:
	var result := Vector2(0,0)
	result.x = grid.size()
	result.y = grid[0].size() if result.x > 0 else 0
	return result

func get_adjacent_tile(tile:FloorTile, dir:Dir) -> FloorTile:
	var adjacent_offset = tile.coord + dir_to_offset(dir)
	return get_tile(adjacent_offset)

func _ready() -> void:
	load_level("edited_level")

func tile_select(coord:Vector2) -> void:
	if play_mode: return

	if selected_tile:
		selected_tile.change_selected(false)
	var tile = get_tile(coord)
	tile.change_selected(true)
	selected_tile = tile


func tile_change(coord:Vector2) -> void:
	if play_mode: return

	var tile = get_tile(coord)
	tile.toggle_tile_type()
	save_level("edited_level")


func is_dir_pressed() -> bool:
	return Input.is_action_just_pressed("up")\
		or Input.is_action_just_pressed("down") \
		or Input.is_action_just_pressed("right") \
		or Input.is_action_just_pressed("left")


func get_dir_pressed() -> Dir:
	if Input.is_action_just_pressed("up"):
		return Dir.UP;
	if Input.is_action_just_pressed("down"):
		return Dir.DOWN;
	if Input.is_action_just_pressed("right"):
		return Dir.RIGHT;
	if Input.is_action_just_pressed("left"):
		return Dir.LEFT;
	push_error("Dir not pressed, use is_dir_pressed()")
	return Dir.UP;


func _physics_process(_delta: float) -> void:
	if is_dir_pressed():
		var dir = get_dir_pressed()
		if play_mode:
			play_mode.on_input(dir)
		else:
			toggle_wall(selected_tile,dir)


func _process(_delta: float) -> void:
	var current_mouse_position =  get_viewport().get_mouse_position()
	if Input.is_mouse_button_pressed(MOUSE_BUTTON_MIDDLE):
		var cam = $Camera2D
		var mouse_delta = current_mouse_position - last_mouse_position
		cam.position -= mouse_delta / cam.zoom
	last_mouse_position = current_mouse_position

func toggle_wall(tile, dir)-> void:
	tile.toggle_wall(dir)
	var adjacent = get_adjacent_tile(tile, dir)
	if adjacent:
		adjacent.toggle_wall(opposite_dir(dir))
	save_level("edited_level")

func save_level(level_name: String):
	var level = LevelData.new()
	level.size = get_grid_size()

	for x in level.size.x:
		level.walls.push_back([])
		level.types.push_back([])
		for y in level.size.y :
			var tile = get_tile(Vector2(x,y))
			level.walls[x].push_back(tile.walls)
			level.types[x].push_back(tile.tile_type)


	ResourceSaver.save(level, "res://Saves/Levels/%s.res" % level_name)


func load_level(level_name:String) -> void: # edited_level
	selected_tile = null
	var level = load("res://Saves/Levels/%s.res" % level_name) as LevelData
	var tiles = $Tiles

	while tiles.get_child_count() > 0:
		tiles.remove_child(tiles.get_child(tiles.get_child_count()-1))
	selected_tile = null
	grid = []

	for x in level.size.x:
		grid.push_back([])
		for y in level.size.y:
			var tile = tile_scene.instantiate() as FloorTile
			tile.register(x,y,self)
			for dir in 4:
				if not level.walls[x][y][dir]:
					tile.toggle_wall(dir)
				tile.set_tile_type(level.types[x][y])
			grid[x].push_back(tile)
			tiles.add_child(tile)


func _on_play_button_pressed() -> void:
	if play_mode:
		play_mode = null
		load_level("edited_level")
		$CanvasLayer/PlayButton.text = "Play"
	else:
		if selected_tile:
			selected_tile.change_selected(false)
		selected_tile = null
		play_mode = PlayMode.new(self)
		$CanvasLayer/PlayButton.text = "Exit Play"
