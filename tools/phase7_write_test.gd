# Faz 7 Save/Load WRITE path doğrulaması (headless):
#   godot --headless -s tools/phase7_write_test.gd
#
# GDScript, C# ekranlarının button "pressed" sinyallerini tetikleyerek
# gerçek yazma yolunu doğrular:
#   PlayerCreation.OnStartClicked → CareerSaveRepository.BeginCareer (signed=0)
#   ContractBeat (accept) → CareerSaveRepository.CommitCareer (signed=1)
extends SceneTree

const CREATION_PATH := "res://scenes/PlayerCreation.tscn"
const BEAT_PATH := "res://scenes/ContractBeat.tscn"
const SAVE_PATH := "user://career.save"

var _failures := 0

func _initialize() -> void:
	_clear_save()
	await _check_player_creation_writes_save()
	await _check_contract_beat_commits_signed()
	_clear_save()

	if _failures > 0:
		push_error("PHASE7 WRITE TEST: %d FAILED" % _failures)
		quit(1)
	else:
		push_warning("PHASE7 WRITE TEST: ALL PASS")
		quit(0)

func _expect(condition: bool, message: String) -> void:
	if condition:
		print("  ok: ", message)
	else:
		push_error("  FAIL: ", message)
		_failures += 1

func _clear_save() -> void:
	if FileAccess.file_exists(SAVE_PATH):
		DirAccess.remove_absolute(SAVE_PATH)

func _read_save() -> String:
	if not FileAccess.file_exists(SAVE_PATH):
		return ""
	var f: FileAccess = FileAccess.open(SAVE_PATH, FileAccess.READ)
	var t: String = f.get_as_text()
	f.close()
	return t

func _add_scene(path: String) -> Node:
	var packed_scene: PackedScene = load(path) as PackedScene
	var instance: Node = packed_scene.instantiate()
	root.add_child(instance)
	await process_frame
	await process_frame
	return instance

func _cleanup_root() -> void:
	for child in root.get_children():
		child.queue_free()
	await process_frame
	await process_frame

# PlayerCreation: ad gir + rol seç + START bas → save (signed=0) yazılmalı.
func _check_player_creation_writes_save() -> void:
	print("== PlayerCreation writes save (signed=0) ==")
	var creation: Node = await _add_scene(CREATION_PATH)

	# Ad gir.
	(creation.get_node("NameSection/NameInput") as LineEdit).text = "Efe Korkmaz"
	await process_frame

	# Rol kartı seç (0 = Striker).
	var role_card: Button = creation.get_node("RoleSection/RoleStack/RoleCard_0") as Button
	role_card.pressed.emit()
	await process_frame

	# START bas.
	var start: Button = creation.get_node("StartCard") as Button
	start.pressed.emit()
	await process_frame
	await process_frame

	_expect(FileAccess.file_exists(SAVE_PATH), "save file written after PlayerCreation start")
	var content: String = _read_save()
	_expect(content.find("name=Efe Korkmaz") != -1, "save contains player name")
	_expect(content.find("role=Striker") != -1, "save contains role=Striker")
	_expect(content.find("signed=0") != -1, "save signed=0 (career begun, not yet signed)")
	await _cleanup_root()

# ContractBeat: trials→first_ball→offer→accept → save (signed=1).
func _check_contract_beat_commits_signed() -> void:
	print("== ContractBeat commits signed save (signed=1) ==")
	# Önce save olmalı ki CommitCareer no-op olmasın. (PlayerCreation yazdı.)
	_expect(FileAccess.file_exists(SAVE_PATH), "precondition: save exists before ContractBeat")

	var beat: Node = await _add_scene(BEAT_PATH)
	await process_frame

	# Beat 1: trials → first option seç.
	await _click_first_option(beat)
	# Beat 2: first_ball → first option seç.
	await _click_first_option(beat)
	# Beat 3: offer → "accept" (0 index = accept).
	await _click_first_option(beat)
	await process_frame
	await process_frame

	var content: String = _read_save()
	_expect(content.find("signed=1") != -1, "save committed to signed=1 after accept")
	await _cleanup_root()

func _click_first_option(beat: Node) -> void:
	var options: Node = beat.get_node("Options") as Node
	if options.get_child_count() == 0:
		push_error("  (no option button found to click)")
		return
	var btn: Button = options.get_child(0) as Button
	btn.pressed.emit()
	await process_frame
	await process_frame
