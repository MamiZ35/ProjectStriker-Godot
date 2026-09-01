# Faz 5 mobil cilası doğrulaması (headless):
#   godot --headless -s tools/phase5_flow_test.gd
# Kontrol edilenler:
#   - SafeArea: 4 sahnede de kök altında var; headless'te kök anchor'ları bozulmaz
#   - PressFeedback: basışta scale ~0.97'ye iner, ~0.15s içinde geri gelir;
#     devre dışı buton dalgalanmaz; ContractBeat dinamik option'ları da çalışır;
#     çift basış (tween kill) temiz kalır
#   - Font boyutları (Unity SceneBuilder referansıyla uyum):
#     ContractBeat Title 40 / Body 28, PlayerCreation OverTitle 25,
#     TrainingCamp OverTitle 25 / TitleLine 25
extends SceneTree

const MENU_PATH := "res://scenes/MainMenu.tscn"
const CREATION_PATH := "res://scenes/PlayerCreation.tscn"
const BEAT_PATH := "res://scenes/ContractBeat.tscn"
const CAMP_PATH := "res://scenes/TrainingCamp.tscn"

var _failures := 0

func _initialize() -> void:
	_clear_save()  # Faz 7: clean state — save sistemi var, testler bağımsız çalışsın
	await _check_safe_area_nodes()
	await _check_press_feedback_main_menu()
	await _check_press_feedback_dynamic_buttons()
	await _check_double_tap_clean()
	await _check_font_sizes()

	if _failures > 0:
		push_error("PHASE5 FLOW TEST: %d FAILED" % _failures)
		quit(1)
	else:
		push_warning("PHASE5 FLOW TEST: ALL PASS")
		quit(0)

func _expect(condition: bool, message: String) -> void:
	if condition:
		print("  ok: ", message)
	else:
		push_error("  FAIL: ", message)
		_failures += 1

func _clear_save() -> void:
	if FileAccess.file_exists("user://career.save"):
		DirAccess.remove_absolute("user://career.save")

func _add_scene(path: String) -> Control:
	var packed_scene: PackedScene = load(path) as PackedScene
	var instance: Node = packed_scene.instantiate()
	root.add_child(instance)
	await process_frame
	await process_frame
	return instance as Control

func _cleanup_root() -> void:
	for child in root.get_children():
		child.queue_free()
	await process_frame
	await process_frame

# ---------------------------------------------------------------- SafeArea --

func _check_safe_area_nodes() -> void:
	print("== safe area nodes ==")
	for scene_path in [MENU_PATH, CREATION_PATH, BEAT_PATH, CAMP_PATH]:
		var screen: Control = await _add_scene(scene_path)
		_expect(screen.get_node_or_null("SafeArea") != null,
				"SafeArea node on %s" % screen.name)
		# Headless'te ScreenGetUsableRect fallback: kök tam ekran kalmalı.
		_expect(screen.anchor_left == 0.0 and screen.anchor_right == 1.0,
				"%s root anchor_left/right unchanged (headless)" % screen.name)
		_expect(screen.anchor_top == 0.0 and screen.anchor_bottom == 1.0,
				"%s root anchor_top/bottom unchanged (headless)" % screen.name)
		await _cleanup_root()

# ------------------------------------------------------------ PressFeedback --

func _check_press_feedback_main_menu() -> void:
	print("== press feedback (scene buttons) ==")
	var menu: Control = await _add_scene(MENU_PATH)

	var new_career: Button = menu.get_node("CTAStack/NewCareerCard") as Button
	new_career.button_down.emit()
	await _wait_seconds(0.05)
	_expect(new_career.scale.x < 0.995, "NEW CAREER scale dips on button_down")

	var cont: Button = menu.get_node("CTAStack/ContinueCard") as Button
	cont.button_down.emit()
	await process_frame
	_expect(cont.scale.x == 1.0, "disabled CONTINUE does not pulse")

	# ~0.30sn beklenince scale geri gelmeli (animasyon 0.12sn).
	await _wait_seconds(0.3)
	_expect(new_career.scale.x == 1.0, "NEW CAREER scale restored after press")
	await _cleanup_root()

func _check_press_feedback_dynamic_buttons() -> void:
	print("== press feedback (dynamic ContractBeat options) ==")
	var beat: Control = await _add_scene(BEAT_PATH)
	var options: Node = beat.get_node("Options") as Node
	var first_option: Button = options.get_child(0) as Button
	_expect(first_option != null, "first option button exists")

	first_option.button_down.emit()
	await _wait_seconds(0.05)
	_expect(first_option.scale.x < 0.995, "dynamic option scale dips on button_down")

	await _wait_seconds(0.3)
	_expect(first_option.scale.x == 1.0, "dynamic option scale restored")
	await _cleanup_root()

func _check_double_tap_clean() -> void:
	print("== double tap (tween kill) ==")
	var camp: Control = await _add_scene(CAMP_PATH)
	var card: Button = camp.get_node("ChoiceSection/ChoiceStack/ChoiceCard_0") as Button

	card.button_down.emit()
	await process_frame
	card.button_down.emit()  # ikinci basış önceki tween'i öldürmeli
	await _wait_seconds(0.05)
	_expect(card.scale.x < 0.995, "second tap restarts pulse (no stuck tween)")

	await _wait_seconds(0.3)
	_expect(card.scale.x == 1.0, "scale clean after double tap")
	await _cleanup_root()

# ------------------------------------------------------------------ Fonts ---

func _check_font_sizes() -> void:
	print("== font sizes (Unity reference parity) ==")
	var beat: Control = await _add_scene(BEAT_PATH)
	_expect((beat.get_node("Title") as Label).get_theme_font_size("font_size") == 40,
			"ContractBeat Title 40px")
	_expect((beat.get_node("Body") as Label).get_theme_font_size("font_size") == 28,
			"ContractBeat Body 28px")
	await _cleanup_root()

	var creation: Control = await _add_scene(CREATION_PATH)
	_expect((creation.get_node("OverTitle") as Label).get_theme_font_size("font_size") == 25,
			"PlayerCreation OverTitle 25px")
	await _cleanup_root()

	var camp: Control = await _add_scene(CAMP_PATH)
	_expect((camp.get_node("OverTitle") as Label).get_theme_font_size("font_size") == 25,
			"TrainingCamp OverTitle 25px")
	_expect((camp.get_node("TitleLine") as Label).get_theme_font_size("font_size") == 25,
			"TrainingCamp TitleLine 25px")
	await _cleanup_root()

# ---------------------------------------------------------------- Helpers --

func _wait_seconds(seconds: float) -> void:
	var timer: SceneTreeTimer = create_timer(seconds)
	await timer.timeout
