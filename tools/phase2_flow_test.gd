# Faz 2+3+4 akış doğrulaması (headless):
#   godot --headless -s tools/phase2_flow_test.gd
# Kontrol edilenler (docs/12 + DEC-018/D-105 + D-007):
#   - TrainingCamp: karşılama + 2 tavır kartı, kapanış beat'i seçim tonuyla açılır,
#     İLK MAÇ ufuk kartı Button taşımaz (D-105), MENÜ → MainMenu (Faz 2 stub)
#   - Köprü (DEC-018): ContractBeat imzalı son → İLK ANTRENMAN → TrainingCamp
#   - PlayerCreation: 4 rol kartı, ön-dolu ad, shuffle, CTA rol seçilene dek
#     devre dışı (D-007), rol seçimi → altın etiket + chevron + çözülmüş kartlar,
#     CTA → ContractBeat
extends SceneTree

const CAMP_PATH := "res://scenes/TrainingCamp.tscn"
const BEAT_PATH := "res://scenes/ContractBeat.tscn"
const CREATION_PATH := "res://scenes/PlayerCreation.tscn"
const MENU_PATH := "res://scenes/MainMenu.tscn"

var _failures := 0

func _initialize() -> void:
	_clear_save()
	await _check_choice_flow(0, "stay")
	await _check_choice_flow(1, "room")
	await _check_menu_button()
	await _check_contract_bridge()
	await _check_player_creation()
	await _check_main_menu()

	if _failures > 0:
		push_error("PHASE2-4 FLOW TEST: %d FAILED" % _failures)
		quit(1)
	else:
		push_warning("PHASE2-4 FLOW TEST: ALL PASS")
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

func _find_button(container: Node, button_text: String) -> Button:
	for child in container.get_children():
		if child is Button and child.text == button_text:
			return child as Button
		var found: Node = _find_button(child, button_text)
		if found != null:
			return found as Button
	return null

func _click_button(container: Node, button_text: String) -> void:
	var button: Button = _find_button(container, button_text)
	if button != null:
		button.pressed.emit()
	else:
		_failures += 1
		print("  (button not found: ", button_text, ")")

func _check_choice_flow(index: int, choice_id: String) -> void:
	print("== choice flow (", choice_id, ") ==")
	var camp: Control = await _add_scene(CAMP_PATH)

	_expect(camp.get_node("ClosingRoot").visible == false, "closing beat hidden before choice")
	_expect(camp.get_node("ChoiceSection").visible, "choice section visible before choice")

	var stack: Node = camp.get_node("ChoiceSection/ChoiceStack") as Node
	_expect(stack.get_child_count() == 2, "two attitude cards")

	(stack.get_child(index) as Button).pressed.emit()
	await process_frame

	_expect(camp.get_node("ChoiceSection").visible == false, "choice section hidden after choice")
	_expect(camp.get_node("ClosingRoot").visible, "closing beat visible after choice")

	var closing_label: Label = camp.get_node("ClosingRoot/ClosingCard/ClosingText") as Label
	var closing_text: String = closing_label.text
	if choice_id == "stay":
		_expect(closing_text.begins_with("Tekralar"), "stay choice -> stay closing tone")
	else:
		_expect(closing_text.begins_with("Soyunma"), "room choice -> room closing tone")

	# D-105: ufuk kartı Button taşımaz — yalnızca Label'lar.
	var horizon: Node = camp.get_node("ClosingRoot/HorizonCard") as Node
	_expect(horizon.visible, "horizon card visible after choice")
	for child in horizon.get_children():
		_expect(child is Label, "horizon card child is Label (D-105)")

	# Tek seçim: ikinci basış kapanışı değiştirmemeli.
	(stack.get_child(1 - index) as Button).pressed.emit()
	await process_frame
	_expect((camp.get_node("ClosingRoot/ClosingCard/ClosingText") as Label).text == closing_text,
			"second press ignored (single choice)")

	await _cleanup_root()

func _check_menu_button() -> void:
	print("== menu button ==")
	var camp: Control = await _add_scene(CAMP_PATH)
	(camp.get_node("ChoiceSection/ChoiceStack/ChoiceCard_0") as Button).pressed.emit()
	await process_frame

	(camp.get_node("ClosingRoot/MenuButton") as Button).pressed.emit()
	for i in 5:
		await process_frame

	_expect(root.get_node_or_null("MainMenuScreen") != null, "scene changed to MainMenu")
	await _cleanup_root()

func _check_contract_bridge() -> void:
	print("== contract -> training bridge (DEC-018) ==")
	var beat: Control = await _add_scene(BEAT_PATH)
	var options: Node = beat.get_node("Options") as Node

	_click_button(options, "Topuk paslarıyla kendini göstermeye çalış")
	await process_frame
	_click_button(options, "Ortayı ara — risk senin olsun")
	await process_frame
	_click_button(options, "Kalemi uzat, imzala")
	await process_frame

	var nav: Button = _find_button(options, "İLK ANTRENMAN")

	_expect(nav != null, "İLK ANTRENMAN affordance on signed ending")
	if nav != null:
		nav.pressed.emit()
		for i in 5:
			await process_frame
		_expect(root.get_node_or_null("TrainingCampScreen") != null,
				"scene changed to TrainingCamp")
	await _cleanup_root()

func _check_main_menu() -> void:
	print("== main menu ==")
	# Faz 7: önceki test (player creation CTA) bir save yazmış olabilir;
	# "CONTINUE disabled" kontrolü için temiz state gerekir.
	_clear_save()
	var menu: Control = await _add_scene(MENU_PATH)

	# Üç CTA kartı, yalnızca NEW CAREER aktif (D-005).
	var stack: Node = menu.get_node("CTAStack") as Node
	_expect(stack.get_child_count() == 3, "three CTA cards")
	var new_career: Button = menu.get_node("CTAStack/NewCareerCard") as Button
	var cont: Button = menu.get_node("CTAStack/ContinueCard") as Button
	var settings: Button = menu.get_node("CTAStack/SettingsCard") as Button
	_expect(not new_career.disabled, "NEW CAREER active")
	_expect(cont.disabled, "CONTINUE disabled")
	_expect(settings.disabled, "SETTINGS disabled")

	# NEWS yalnızca üst-sol rozet (kart değil).
	_expect(menu.get_node("TopBar/NewsChip/Label") != null, "NEWS chip present")

	# NEW CAREER → PlayerCreation (DEC-017).
	new_career.pressed.emit()
	for i in 5:
		await process_frame
	_expect(root.get_node_or_null("PlayerCreationScreen") != null,
			"NEW CAREER loads PlayerCreation")
	await _cleanup_root()

func _check_player_creation() -> void:
	print("== player creation ==")
	var pc: Control = await _add_scene(CREATION_PATH)

	var stack: Node = pc.get_node("RoleSection/RoleStack") as Node
	_expect(stack.get_child_count() == 4, "four role cards")

	var name_input: LineEdit = pc.get_node("NameSection/NameInput") as LineEdit
	_expect(name_input.text != "", "name input pre-filled")
	_expect(name_input.max_length == 24, "name character limit 24 (D-001)")

	var start: Button = pc.get_node("StartCard") as Button
	_expect(start.disabled, "CTA disabled before role selection")

	# Shuffle: sentinel + shuffle -> dolu, sınır içinde, farklı.
	name_input.text = "SENTINEL_TEST_AD"
	(pc.get_node("NameSection/ShuffleButton") as Button).pressed.emit()
	await process_frame
	_expect(name_input.text != "SENTINEL_TEST_AD" and name_input.text != "",
			"shuffle produces new proposal")
	_expect(name_input.text.length() <= 24, "shuffle within 24 limit")

	# D-007: rol seçimi -> CTA etkin, seçili kart vurgulu (altın etiket + chevron),
	# diğerleri ~%45'e çözülür.
	var first_label: Label = (stack.get_child(0) as Control).get_node("Label") as Label
	var first_chevron: TextureRect = (stack.get_child(0) as Control).get_node("Chevron") as TextureRect
	_expect(first_chevron.visible == false, "chevron hidden before selection")
	(stack.get_child(0) as Button).pressed.emit()
	await process_frame
	_expect(not start.disabled, "CTA enabled after role selection")
	_expect(first_chevron.visible, "selected card chevron visible")
	var gold_color: Color = Color(0.788235, 0.658824, 0.298039, 1)
	_expect(first_label.get_theme_color("font_color").is_equal_approx(gold_color),
			"selected card label gold")
	_expect((stack.get_child(1) as Control).modulate.a < 0.5,
			"unselected card dimmed ~45%")
	_expect((stack.get_child(0) as Control).modulate.a > 0.99,
			"selected card full alpha")

	# Tek-seçim: ikinci kart seçilirse ilk vurgu düşer.
	(stack.get_child(1) as Button).pressed.emit()
	await process_frame
	_expect(first_chevron.visible == false, "old card chevron hidden after switch")
	_expect((stack.get_child(0) as Control).modulate.a < 0.5,
			"old card dimmed after switch")

	# CTA -> ContractBeat (DEC-010).
	start.pressed.emit()
	for i in 5:
		await process_frame
	_expect(root.get_node_or_null("ContractBeatScreen") != null,
			"CTA loads ContractBeat")
	await _cleanup_root()
