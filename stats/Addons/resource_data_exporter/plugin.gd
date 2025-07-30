@tool
extends EditorPlugin

var exporter_instance

func _enter_tree():
	# 创建导出器实例
	exporter_instance = preload("res://Scripts/Editor/ResourceDataExporter.cs").new()
	# 添加到编辑器插件系统
	add_child(exporter_instance)

func _exit_tree():
	# 清理
	if exporter_instance:
		remove_child(exporter_instance)
		exporter_instance.queue_free()