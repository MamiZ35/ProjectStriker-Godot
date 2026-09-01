# Faz 7 Save/Load doğrulaması (headless):
#   godot --headless -s tools/phase7_flow_test.gd
#
# C# tarafı (CareerSaveRepository/CareerSave) motor-bağımsız mantıkla üretilir;
# bu test, MainMenuScreen'in (C#) disktaki kariyer kaydını doğru okuyup
# CONTINUE kartını aktif ettiğini doğrular. Save dosyası, C#'ın CareerSave
# formatıyla birebir aynı formatla GDScript üzerinden yazılır — böylece
# format tutarlılığı da test edilmiş olur.
extends SceneTree

const MENU_PATH := "res://scenes/MainMenu.tscn"
const SAVE_PATH := "user://career.save"

const FORMAT_VERSION := "striker.save.v1"

var _failures := 0

func _initialize() -> void:
	_clear_save()
	await _check_no_save_continue_disabled()
	await _check_signed_save_continue_enabled()
	await _check_unsigned_save_continue_enabled()
	_clear_save()

	if _failures > 0:
		push_error("PHASE7 FLOW TEST: %d FAILED" % _failures)
		quit(1)
	else:
		push_warning("PHASE7 FLOW TEST: ALL PASS")
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

func _write_save(signed: bool) -> void:
	var f: FileAccess = FileAccess.open(SAVE_PATH, FileAccess.WRITE)
	f.store_string(FORMAT_VERSION + "\n")
	f.store_string("name=Efe Korkmaz\n")
	f.store_string("role=Striker\n")
	f.store_string("signed=" + ("1" if signed else "0") + "\n")
	f.close()

func _load_menu_continue() -> Button:
	var packed_scene: PackedScene = load(MENU_PATH) as PackedScene
	var instance: Node = packed_scene.instantiate()
	root.add_child(instance)
	await process_frame
	await process_frame
	return instance.get_node("CTAStack/ContinueCard") as Button

func _cleanup_root() -> void:
	for child in root.get_children():
		child.queue_free()
	await process_frame
	await process_frame

func _check_no_save_continue_disabled() -> void:
	print("== no save → CONTINUE disabled ==")
	var cont: Button = await _load_menu_continue()
	_expect(cont != null, "CONTINUE button exists")
	_expect(cont.disabled == true, "CONTINUE disabled when no save")
	await _cleanup_root()

func _check_signed_save_continue_enabled() -> void:
	print("== signed save → CONTINUE enabled ==")
	_write_save(true)
	var cont: Button = await _load_menu_continue()
	_expect(cont != null, "CONTINUE button exists")
	_expect(cont.disabled == false, "CONTINUE enabled when signed save exists")
	_clear_save()
	await _cleanup_root()

func _check_unsigned_save_continue_enabled() -> void:
	print("== unsigned save → CONTINUE enabled ==")
	_write_save(false)
	var cont: Button = await _load_menu_continue()
	_expect(cont != null, "CONTINUE button exists")
	_expect(cont.disabled == false, "CONTINUE enabled when unsigned save exists")
	_clear_save()
	await _cleanup_root()
