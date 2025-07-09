1. 能用代码解决的事情优先用代码解决
2. godot版本：4.4+
3. **Godot场景生成器Owner设置规则**：
   - 必须在所有节点添加到场景树后再设置Owner属性
   - Owner必须是节点在场景树中的祖先节点
   - 使用递归方法统一设置所有子节点的Owner
   - 避免在节点创建函数内部设置Owner，应在主场景构建完成后统一设置
   - 示例模式：先AddChild()，后SetOwner()或SetOwnerRecursively()