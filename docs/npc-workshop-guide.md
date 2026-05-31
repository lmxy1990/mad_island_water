# Mad Island NPC Workshop 中文图文教程

本文根据 Emade Plus 官方页面整理：<https://emadeplus.com/workshop-npc/>

图片来源均为官方教程页面。本文只引用远程图片，不在仓库中重新分发官方素材。

## 准备工作

创建 NPC 需要使用动画软件 **Spine**。

官方提供了两个示例工程：

- 初级：`workshop_dummy_00`
- 中级：`workshop_dummy_01`

注意事项：

- 需要 Spine Pro 以上版本编辑。试用版通常只能读取，不能完整编辑导出。
- **必须使用 Spine 3.8.99 导出。**
- Mad Island 的角色数据是按 Spine 3.8.99 制作的，版本不一致会导致游戏无法读取。

![官方 NPC Workshop 页面](https://emadeplus.com/wp-content/uploads/2025/07/image-6.png)

## Spine 数据规格

### 必需动画

初级示例里包含了最低限度需要的动画。

动画名称必须完全一致，包括大小写。`A_run`、`A_walkback` 不是必需项；如果没有，游戏会用 `A_walk` 代替。

常用动画名：

- `A_walk`
- `A_run`
- `A_walkback`
- `A_sleep`：睡觉
- `B_idle`：坐下
- `E_dance`：跳舞
- `E_danceH`：特殊舞蹈
- `A_work_...`：工作时动画等

骨骼数量和骨骼名称没有固定限制。

### 装备与插槽

如果想让 NPC 支持道具装备，需要让 Spine 的 slot、attachment 名称符合游戏规则。

slot 和 attachment 的数量没有硬性限制，但命名符合规则后，游戏才能进行颜色编辑、装备切换等操作。详细命名规则见本文后面的“高级规格”。

## Spine 导出设置

最终输出结构应与官方示例里的 `toUnity` 文件夹一致。

同一个 NPC 文件夹内需要有 3 类文件：

- `.atlas`
- `.json`
- `.png`

文件名和它们所在的文件夹名必须一致。

示例：

```text
MyNpc/
  MyNpc.atlas
  MyNpc.json
  MyNpc.png
```

![Spine 导出后的文件结构](https://emadeplus.com/wp-content/uploads/2025/07/image-4.png)

## 在游戏中加载 Spine NPC

先进入游戏标题画面：

1. 选择 `Workshop`
2. 选择 `Load Spine Character`

![标题画面进入 Workshop](https://emadeplus.com/wp-content/uploads/2025/07/image-1024x568.png)

加载流程：

1. 点击左下角 `Open NPC Folder` 打开 NPC 文件夹。
2. 把 Spine 导出的 NPC 文件夹整个放进去。
3. 点击 `Load`，游戏会列出文件夹里的角色数据。
4. 在列表里点击要读取的数据。

![加载 NPC 文件夹](https://emadeplus.com/wp-content/uploads/2025/07/image-5-1024x578.png)

![选择已加载的数据](https://emadeplus.com/wp-content/uploads/2025/07/image-2.png)

## 设置 NPC 参数

读取数据后，`File Name` 会显示当前加载的数据名。

Level 0 参数说明：

- `max life`：最大 HP
- `attack`：攻击力
- `speed`：步行速度
- `run speed`：跑步速度倍率。设为 `1` 时，跑步速度和步行速度相同
- `Hit Frame`：攻击判定出现的动画帧。可用 `Space` 检查攻击动画
- `Distance`：靠近到多远开始攻击。网格 1 格为 `1`。目前设置大于 `2` 时，实际也不会超过 `2`
- `ColliderSize`：攻击判定大小
- `ColliderOffset`：攻击判定位置
- `Scale`：角色尺寸

全部设置完成后，点击 `Save Parameters`。

保存后，角色文件夹内会生成：

```text
npcInfo.xml
```

有了这个文件，NPC 参数设置就完成了。

![NPC 参数设置](https://emadeplus.com/wp-content/uploads/2025/07/image-7-1024x578.png)

## 预览操作

在加载与参数设置界面可以用以下操作检查角色：

- `WASD`：移动
- 按住 `Shift` 移动：跑步
- `Space`：播放攻击动画，并在 `Hit Frame` 触发判定
- `Alt + 左键拖动`：旋转视角
- `Alt + 右键拖动`：缩放视角
- 鼠标滚轮拖动：平移视角
- `F`：重置摄像机位置

## 上传到 Steam Workshop

如果只在本地使用 NPC，可以跳过本节。

上传流程：

1. 确认 `File Name` 是要上传的数据。
2. 点击 `Publish` 打开上传面板。
3. 输入作品标题。
4. 点击 `Snapshot` 生成角色预览图。
5. 输入作品说明。
6. 点击 `Publish` 开始上传。

`Snapshot` 上方的 `Scale` 滑条可调整截图比例，按角色大小调整即可。

截图会在 `NPCs` 文件夹内生成，文件名为：

```text
iconImage
```

该图片用于上传，不需要手动修改；如果已经存在，会被覆盖。

上传成功后会显示：

```text
Sucessfully submit item to Steam
```

如果出现红色错误，检查：

- 角色是否已经正确读取
- 是否已经保存参数并生成 `npcInfo.xml`
- 上传面板里的标题、截图、说明是否有漏填

![打开 Publish 面板](https://emadeplus.com/wp-content/uploads/2025/07/image-8-1024x575.png)

![填写标题与截图](https://emadeplus.com/wp-content/uploads/2025/07/image-9-1024x574.png)

![上传说明与发布](https://emadeplus.com/wp-content/uploads/2025/07/image-10-1024x575.png)

## 在游戏中召唤 NPC

目前可以把 Workshop NPC 作为伙伴召唤，或作为敌人生成在附近。

操作流程：

1. 与游戏内 Workshop 物品互动。
2. 点击 `召喚` 按钮。
3. 点击 `Load Files`。
4. 本地 `NPCs` 文件夹中的角色，以及 Steam Workshop 订阅的角色会显示在列表里。
5. 点击要加载的 `File Name`。
6. 读取完成后会显示勾选标记，并分配 `npcID`，编号从 `500` 开始。

加载完成后：

- 点击 `Friend`：在当前位置召唤为伙伴
- 点击 `Enemy`：在稍远的随机位置生成 1 个敌人

![召唤 Workshop NPC](https://emadeplus.com/wp-content/uploads/2025/07/image-12-1024x576.png)

![加载文件并生成 NPC](https://emadeplus.com/wp-content/uploads/2025/07/image-15-1024x575.png)

## 解除读取

再次点击已经读取的 NPC `FileName` 按钮，可以解除读取。

注意：如果 `npcID 500` 的角色已经作为伙伴存在，此时解除 `npcID 500` 的读取并保存，之后读档时，所有原本以 `npcID 500` 存在的角色都会消失。

## 更新已发布的 Workshop 物品

更新流程：

1. 点击 `Publish` 打开面板。
2. 点击 `Update`，会列出你自己上传过的 Workshop 物品。
3. 点击要更新的数据。
4. 和上传时一样填写 `Title`、`Snapshot`、`Description`。
5. 点击 `Update` 完成更新。

![更新 Workshop 物品](https://emadeplus.com/wp-content/uploads/2025/07/image-17-1024x573.png)

## 高级规格：颜色编辑 slot 名称

游戏会根据 Spine 的 slot 名称寻找可变色部位。

### 肤色

```text
Head, Head_H, Neck, Face, Nose, L_arm, L_forearm, L_hand, R_arm, R_hand, R_forearm,
L_upleg, L_leg, R_upleg, R_leg, Body, BodyBase, Body_up, Body_preg, Hip, L_hip, R_hip,
L_finger, R_finger, Ear, EarBack, L_manko, R_manko, L_Oppai, R_Oppai, Oppai, Anal,
Ass_up, Ass_down, L_Hip, R_Hip, Body_L_damage, Body_R_damage, L_foot, R_foot, Hips,
Head2, Cloth_Oppai_up, Cloth_Oppai_low, L_Rope, L_Rope2, Tama, Tinko, Jaw, Mouth_up
```

### 头发与眼睛

- 前发：`HairFront`
- 后发：`HairBack`
- 眉毛：`EyeBrow`
- 白眼：`EyeWhite`
- 瞳孔：`Eye`

## 高级规格：装备 slot 与 attachment

装备会检查对应 slot 和 attachment 名称是否存在。

装备物品有自己的 ID，游戏通常会寻找：

```text
槽位名_编号
```

例：漂亮石项链的 ID 是 `acce_00`，类型编号是 `00`。要让 NPC 支持这个饰品，至少需要存在以下之一：

```text
Acce slot 内有 Acce_00 attachment
Acce_H slot 内有 Acce_H_00 attachment
```

### 饰品

```text
Acce, AcceBack, AcceBack2, Acce_L, Acce_H, Acce_H2
```

### 帽子

```text
Hat, HatBack, Hat_L, Hat_H
```

### 手 1：武器

```text
Tool, Tool_R, Tool_H
```

武器 attachment 名称有例外：武器 ID 会去掉开头的 `wp_`。

例：石斧 ID 是 `wp_axe_01`，则 `Tool` slot 里需要：

```text
axe_01
```

### 手 2：火把等

```text
Tool2, Shield
```

### 上衣

```text
Body_Cloth, Body_Cloth_L, Body_Cloth_H, Body_ClothBack, Body_L_Cloth, Body_R_Cloth,
R_arm_Cloth, L_arm_Cloth, R_forearm_Cloth, L_forearm_Cloth, L_hand_Cloth,
L_finger_Cloth, R_hand_Cloth, R_finger_Cloth
```

上衣 attachment 有例外：`Body_Cloth` slot 里 attachment 名称为：

```text
Body_Cloth_a_编号
```

### 下装

```text
Hip_Cloth, Hip_L_Cloth, Hip_R_Cloth, Hip_Cloth_H, Hip_Cloth_L, Hip_Cloth_L2,
L_upleg_Cloth, R_upleg_Cloth, L_leg_Cloth, R_leg_Cloth
```

### 鞋

```text
L_shoe, R_shoe, L_shoe_L, R_shoe_L, L_shoe_H, R_shoe_H
```

### 尾部/臀部装饰

```text
Tail
```

## 常见问题

### 游戏不读取 NPC

优先检查：

- Spine 是否用 `3.8.99` 导出
- `.atlas`、`.json`、`.png` 文件名是否和文件夹名一致
- 文件是否放在 `Open NPC Folder` 打开的目录下，并且是整个文件夹放进去
- 动画名是否大小写完全一致

### 参数没有保存

检查角色是否已加载，并点击过 `Save Parameters`。成功后角色文件夹内应该有 `npcInfo.xml`。

### 上传失败

检查：

- `File Name` 是否是当前要上传的角色
- 是否生成了 `npcInfo.xml`
- 上传面板是否填写了标题、截图和说明
- Steam 是否正常登录

