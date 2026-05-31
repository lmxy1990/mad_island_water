from pathlib import Path

from PIL import Image, ImageDraw, ImageFont


ROOT = Path(__file__).resolve().parent
ORIGINAL = ROOT / "original"
FONT_REGULAR = Path(r"C:\Windows\Fonts\msyh.ttc")
FONT_BOLD = Path(r"C:\Windows\Fonts\msyhbd.ttc")


def load_font(size, bold=False):
    font_path = FONT_BOLD if bold and FONT_BOLD.exists() else FONT_REGULAR
    if font_path.exists():
        return ImageFont.truetype(str(font_path), size)
    return ImageFont.load_default()


def text_box(img, draw, box, text, size=20, bg=(0, 0, 0, 215), fg=(255, 255, 255, 255),
             bold=False, radius=4, align="center", stroke=1, outline=None):
    font = load_font(size, bold)
    layer = Image.new("RGBA", img.size, (0, 0, 0, 0))
    layer_draw = ImageDraw.Draw(layer)
    if radius:
        layer_draw.rounded_rectangle(box, radius=radius, fill=bg, outline=outline)
    else:
        layer_draw.rectangle(box, fill=bg, outline=outline)
    img.alpha_composite(layer)

    x1, y1, x2, y2 = box
    bbox = draw.textbbox((0, 0), text, font=font, stroke_width=stroke)
    width = bbox[2] - bbox[0]
    height = bbox[3] - bbox[1]
    if align == "left":
        x = x1 + 6
    elif align == "right":
        x = x2 - width - 6
    else:
        x = x1 + (x2 - x1 - width) / 2
    y = y1 + (y2 - y1 - height) / 2 - 1
    draw.text((x, y), text, font=font, fill=fg, stroke_width=stroke,
              stroke_fill=(0, 0, 0, 180))


def apply_labels(filename, labels):
    img = Image.open(ORIGINAL / filename).convert("RGBA")
    draw = ImageDraw.Draw(img)
    for label in labels:
        text_box(img, draw, **label)
    img.convert("RGB").save(ROOT / filename, optimize=True)


def l(box, text, size=20, bg=(0, 0, 0, 215), fg=(255, 255, 255, 255),
      bold=False, radius=4, align="center", stroke=1, outline=None):
    return {
        "box": box,
        "text": text,
        "size": size,
        "bg": bg,
        "fg": fg,
        "bold": bold,
        "radius": radius,
        "align": align,
        "stroke": stroke,
        "outline": outline,
    }


GREEN = (28, 122, 65, 235)
BROWN = (86, 70, 36, 235)
BLUE = (39, 65, 145, 238)
PURPLE = (128, 45, 147, 238)
PANEL = (0, 0, 0, 205)
SOFT = (0, 0, 0, 165)
TEAL = (44, 122, 107, 220)


COMMON_EDITOR = [
    l((58, 36, 168, 58), "状态设置", 18, bg=TEAL, bold=True),
    l((374, 10, 443, 34), "文件名", 16, bg=SOFT),
    l((758, 7, 839, 36), "发布", 18, bg=BLUE, bold=True),
    l((896, 15, 987, 40), "Spine 数据", 17, bg=SOFT, bold=True),
    l((853, 43, 925, 70), "加载", 17, bg=PANEL, bold=True),
    l((21, 66, 66, 90), "名称", 15, bg=SOFT),
    l((20, 126, 85, 151), "最大生命", 15, bg=SOFT),
    l((21, 156, 70, 181), "攻击", 15, bg=SOFT),
    l((21, 186, 70, 210), "速度", 15, bg=SOFT),
    l((20, 215, 92, 240), "跑速", 15, bg=SOFT),
    l((64, 249, 181, 270), "攻击详情", 15, bg=SOFT),
    l((20, 276, 98, 301), "命中帧", 15, bg=SOFT),
    l((20, 307, 84, 333), "距离", 15, bg=SOFT),
    l((20, 336, 105, 363), "碰撞大小", 15, bg=SOFT),
    l((101, 336, 158, 359), "碰撞", 14, bg=(39, 96, 116, 235)),
    l((20, 396, 116, 423), "碰撞偏移", 15, bg=SOFT),
    l((226, 408, 277, 449), "尺寸\n示例", 12, bg=PANEL, bold=True),
    l((228, 450, 276, 474), "网格", 16, bg=PANEL, bold=True),
    l((235, 484, 284, 508), "缩放", 15, bg=SOFT),
    l((44, 462, 181, 488), "保存参数", 17, bg=GREEN, bold=True),
    l((43, 500, 181, 528), "打开NPC文件夹", 16, bg=BROWN, bold=True),
    l((3, 548, 84, 575), "返回标题", 14, bg=PANEL),
    l((137, 552, 206, 575), "工坊指南", 13, bg=SOFT),
]


PUBLISH_PANEL = [
    l((564, 37, 623, 63), "更新", 16, bg=PURPLE, bold=True),
    l((633, 39, 681, 60), "标题", 15, bg=SOFT),
    l((635, 205, 685, 226), "缩放", 14, bg=SOFT),
    l((669, 232, 784, 259), "截图", 18, bg=GREEN, bold=True),
    l((633, 267, 726, 288), "说明", 15, bg=SOFT, align="left"),
    l((637, 420, 815, 457), "发布", 24, bg=BLUE, bold=True),
    l((752, 481, 832, 507), "关闭", 16, bg=PANEL, bold=True),
]


WORKSHOP_NPC_PANEL = [
    l((773, 46, 875, 77), "加载文件", 18, bg=PANEL, bold=True),
    l((665, 88, 712, 106), "已加载", 13, bg=SOFT),
    l((716, 88, 780, 106), "文件名", 13, bg=SOFT),
    l((848, 88, 894, 106), "NPC ID", 13, bg=SOFT),
    l((910, 88, 962, 106), "生成", 13, bg=SOFT),
]


for y in (110, 142, 174, 206):
    WORKSHOP_NPC_PANEL.append(l((888, y, 934, y + 21), "伙伴", 13, bg=PANEL, bold=True))
    WORKSHOP_NPC_PANEL.append(l((940, y, 986, y + 21), "敌人", 13, bg=PANEL, bold=True))


def main():
    apply_labels("01-official-page.png", [
        l((1, 1, 112, 24), "导出", 18, bg=(42, 90, 120, 235), bold=True),
        l((8, 35, 70, 57), "数据", 15, bg=SOFT),
        l((100, 35, 217, 57), "JSON 导出", 15, bg=SOFT),
        l((137, 62, 229, 84), "输出文件夹", 15, bg=SOFT),
        l((178, 91, 226, 113), "扩展名", 14, bg=SOFT),
        l((178, 121, 226, 143), "格式", 14, bg=SOFT),
        l((224, 151, 274, 173), "输出", 14, bg=SOFT),
        l((226, 236, 337, 260), "打包设置", 16, bg=(75, 75, 75, 240), bold=True),
        l((10, 346, 99, 374), "保存", 15, bg=(92, 92, 92, 235), bold=True),
        l((107, 346, 196, 374), "读取", 15, bg=(92, 92, 92, 235), bold=True),
        l((213, 347, 368, 374), "导出后打开", 14, bg=SOFT),
        l((376, 346, 487, 374), "导出", 15, bg=(92, 92, 92, 235), bold=True),
        l((497, 346, 602, 374), "取消", 15, bg=(92, 92, 92, 235), bold=True),
    ])

    apply_labels("02-export-files.png", [
        l((1, 1, 151, 24), "纹理打包设置", 17, bg=(42, 90, 120, 235), bold=True),
        l((9, 34, 68, 56), "区域", 15, bg=SOFT),
        l((359, 34, 451, 56), "区域填充", 15, bg=SOFT),
        l((9, 150, 70, 172), "页面", 15, bg=SOFT),
        l((360, 150, 420, 172), "渲染", 15, bg=SOFT),
        l((10, 328, 58, 350), "输出", 15, bg=SOFT),
        l((419, 333, 470, 354), "选项", 15, bg=SOFT),
        l((276, 214, 398, 238), "图像文件夹", 14, bg=(115, 115, 115, 235)),
        l((10, 512, 98, 540), "保存", 15, bg=(92, 92, 92, 235), bold=True),
        l((107, 512, 195, 540), "读取", 15, bg=(92, 92, 92, 235), bold=True),
        l((205, 512, 304, 540), "默认", 15, bg=(92, 92, 92, 235), bold=True),
        l((442, 512, 549, 540), "确定", 15, bg=(92, 92, 92, 235), bold=True),
        l((560, 512, 646, 540), "取消", 15, bg=(92, 92, 92, 235), bold=True),
    ])

    apply_labels("03-workshop-menu.png", [
        l((78, 181, 210, 214), "继续", 26, bg=SOFT, bold=True),
        l((78, 232, 210, 264), "开始", 26, bg=SOFT, bold=True),
        l((78, 281, 210, 313), "编辑", 26, bg=SOFT, bold=True),
        l((78, 329, 222, 361), "选项", 26, bg=SOFT, bold=True),
        l((84, 371, 224, 402), "创意工坊", 24, bg=SOFT, bold=True),
        l((78, 418, 166, 451), "结束", 26, bg=SOFT, bold=True),
        l((274, 323, 386, 357), "新建", 26, bg=PANEL, bold=True),
        l((274, 371, 565, 402), "加载 Spine 角色", 25, bg=PANEL, bold=True),
        l((276, 418, 427, 451), "已订阅", 25, bg=PANEL, bold=True),
    ])

    apply_labels("04-load-folder.png", COMMON_EDITOR)
    apply_labels("05-select-data.png", [
        l((1, 1, 43, 25), "共享", 14, bg=(245, 245, 245, 240), fg=(35, 35, 35, 255), stroke=0),
        l((48, 1, 91, 25), "查看", 14, bg=(245, 245, 245, 240), fg=(35, 35, 35, 255), stroke=0),
        l((0, 69, 62, 93), "剪贴板", 13, bg=(245, 245, 245, 240), fg=(35, 35, 35, 255), stroke=0),
        l((229, 108, 301, 132), "整理", 13, bg=(245, 245, 245, 240), fg=(35, 35, 35, 255), stroke=0),
        l((379, 41, 447, 88), "新建\n文件夹", 13, bg=(245, 245, 245, 240), fg=(35, 35, 35, 255), stroke=0),
        l((438, 30, 526, 64), "新建项目", 13, bg=(245, 245, 245, 240), fg=(35, 35, 35, 255), stroke=0),
        l((545, 30, 585, 64), "属性", 13, bg=(245, 245, 245, 240), fg=(35, 35, 35, 255), stroke=0),
        l((596, 28, 645, 53), "打开", 13, bg=(245, 245, 245, 240), fg=(35, 35, 35, 255), stroke=0),
        l((664, 31, 732, 53), "全选", 13, bg=(245, 245, 245, 240), fg=(35, 35, 35, 255), stroke=0),
        l((101, 163, 140, 183), "名称", 13, bg=(255, 255, 255, 245), fg=(45, 45, 45, 255), stroke=0),
        l((369, 164, 435, 184), "修改日期", 13, bg=(255, 255, 255, 245), fg=(45, 45, 45, 255), stroke=0),
        l((511, 164, 548, 184), "类型", 13, bg=(255, 255, 255, 245), fg=(45, 45, 45, 255), stroke=0),
        l((630, 164, 668, 184), "大小", 13, bg=(255, 255, 255, 245), fg=(45, 45, 45, 255), stroke=0),
        l((677, 130, 756, 154), "搜索 Npcs", 13, bg=(255, 255, 255, 245), fg=(110, 110, 110, 255), stroke=0),
    ])
    apply_labels("06-parameters.png", COMMON_EDITOR)
    apply_labels("07-publish-panel.png", COMMON_EDITOR + PUBLISH_PANEL)
    apply_labels("08-snapshot-title.png", COMMON_EDITOR + PUBLISH_PANEL + [
        l((332, 45, 694, 80), "已成功提交到 Steam", 22, bg=(0, 0, 0, 130),
          fg=(111, 245, 255, 255), bold=True),
    ])

    apply_labels("09-description-publish.png", [
        l((212, 457, 512, 488), "创意工坊", 24, bg=PANEL, bold=True),
        l((549, 198, 626, 221), "外出", 14, bg=SOFT, bold=True),
        l((549, 224, 626, 248), "召唤", 14, bg=SOFT, bold=True),
    ])

    apply_labels("10-summon-menu.png", WORKSHOP_NPC_PANEL)
    apply_labels("11-load-files.png", WORKSHOP_NPC_PANEL)

    apply_labels("12-update-item.png", COMMON_EDITOR + [
        l((400, 42, 512, 63), "已发布项目", 15, bg=SOFT),
        l((463, 449, 543, 474), "关闭", 16, bg=PANEL, bold=True),
        l((564, 37, 623, 63), "更新", 16, bg=PURPLE, bold=True),
        l((633, 39, 681, 60), "标题", 15, bg=SOFT),
        l((657, 126, 789, 162), "无图片", 28, bg=PANEL, bold=True),
        l((635, 205, 685, 226), "缩放", 14, bg=SOFT),
        l((669, 232, 784, 259), "截图", 18, bg=GREEN, bold=True),
        l((633, 267, 726, 288), "说明", 15, bg=SOFT, align="left"),
        l((637, 420, 815, 457), "更新", 24, bg=PURPLE, bold=True),
        l((752, 481, 832, 507), "关闭", 16, bg=PANEL, bold=True),
    ])


if __name__ == "__main__":
    main()
