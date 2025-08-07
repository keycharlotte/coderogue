@tool
extends EditorPlugin

var exporter_instance

func _enter_tree():
	# 创建导出器实例
	var script = load("res://Scripts/Editor/ResourceDataExporter.cs")
	if script:
		exporter_instance = script.new()
		# 添加到编辑器插件系统
		add_child(exporter_instance)
	else:
		print("Failed to load ResourceDataExporter.cs")

func _exit_tree():
	# 清理
	if exporter_instance:
		remove_child(exporter_instance)
		exporter_instance.queue_free()