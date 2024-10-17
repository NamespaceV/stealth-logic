class_name FloorTile
extends Node2D

var coord : Vector2
var mgr:  GameManager

var selected := false

enum TileType
{
	EMPTY,
	PLAYER,
	ENEMY,
}

@export var selectedTex : Texture2D
var defaultTex : Texture2D

@export var walls = [true, true, true, true]

var tile_type:TileType

func register(tile_x,tile_y,game_manager: GameManager):
	mgr = game_manager
	coord.x = tile_x
	coord.y = tile_y
	name = "tile_%d_%d"%[tile_x,tile_y]
	position = coord * 256
	defaultTex = $BG.texture

func _to_string() -> String:
	return name

func _on_area_2d_input_event(_viewport: Node, event: InputEvent, _shape_idx: int) -> void:
	# print(name, "->", event)
	if event is InputEventMouse and event.is_pressed() \
			and event.button_index == MOUSE_BUTTON_LEFT:
		# print(name, "->", event)
		mgr.tile_select(coord)
	if event is InputEventMouse and event.is_pressed() \
			and event.button_index == MOUSE_BUTTON_RIGHT:
		# print(name, "->", event)
		mgr.tile_change(coord)


func can_move(dir):
	return !walls[dir]


func change_selected(val:bool):
	selected = val
	$BG.texture = selectedTex if selected else defaultTex

func toggle_wall(dir : GameManager.Dir):
	var wall = $Walls.get_child(dir) as Polygon2D
	wall.visible = !wall.visible
	walls[dir] = !walls[dir]

func toggle_tile_type():
	set_tile_type((tile_type + 1) % 3 as TileType)

func set_tile_type(type:TileType):
	tile_type = type
	$Interactable/Enemy.visible = type == TileType.ENEMY
	$Interactable/Player.visible = type == TileType.PLAYER
