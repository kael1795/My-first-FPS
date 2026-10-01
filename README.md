# My first FPS

一个使用 Unity 独立开发的第一人称僵尸生存射击 Demo，完成了从射击、换弹、敌人 AI、波次生成到商店经济的完整客户端玩法闭环。

> 个人独立项目，全部客户端逻辑自行设计与实现，约 1900 行 C# 代码。

---

## 项目信息

| 项 | 说明 |
|---|---|
| 引擎版本 | Unity 2022.3.51f1c1（LTS） |
| 开发语言 | C# |
| 主要模块 | 对象池、武器系统、怪物系统、商店系统 |
| 代码量 | 约 1900 行（15 个核心脚本） |
| 主场景 | `Assets/CSAssets2026/Scenes/CityNew.unity` |

## 技术栈

- **导航与 AI**：`NavMeshAgent` + 枚举有限状态机（Idle / Chase / Attack / Dead）
- **数据驱动**：`ScriptableObject` 配置武器与怪物参数
- **性能优化**：通用对象池（预热、容量上限、池化生命周期回调）
- **UI**：UGUI + TextMeshPro
- **物理**：Layer 分层 + `LayerMask` 射线检测过滤

## 已实现功能

### 武器与弹药
- 多武器切换，武器参数（伤害、射速、后坐力、弹匣容量、备弹、音效、子弹预制体）全部外置为 `WeaponData` 资产，新增武器只需添加数据资产、不改动逻辑代码
- 空仓拦截、换弹期间禁止开火、备弹不足时部分装填
- 切枪时按武器 ID 保存/恢复各自的弹匣与备弹状态

### 射击表现
- 弹道按「摄像机视口中心射线求命中点 → 从枪口指向该点」生成，解决准星与弹着点不一致
- 后坐力目标值累加 + 逐帧插值回正，实现连续射击的枪口上抬累积与自动回弹
- 命中时协程驱动准星缩放与透明度渐隐

### 敌人 AI
- 四状态有限状态机，用 `switch` 状态分发替代逐帧 `if-else`，新增状态只需扩展枚举与分支
- 视野距离与攻击距离判定、攻击冷却、状态自动迁移
- 寻路异常处理：启用导航前校验 `NavMeshAgent` 是否在导航网格上，不在则用 `Warp` 校正，并将导航 API 调用统一收敛到就绪判定
- 两类敌人：普通僵尸、自爆蜘蛛（子类重写死亡行为）

### 对象池
- 通用 `ObjectPoolManager`：以预制体为键维护缓存池，支持预热与容量上限（超出即销毁）
- `IPoolable` 接口在出池/入池时统一重置状态（血量、计时、动画、刚体速度、寻路状态、协程），避免复用对象残留上一轮状态
- 子弹与敌人两种高频对象全部走池化

### 波次系统
- 怪物数量逐波递增，刷新间隔逐波缩短并设下限
- 以 `HashSet` 追踪存活怪物，敌人在归还对象池时自动通知波次管理器出队，实现「全歼判定 + 下一波推进」
- 刷新点轮转分配并带随机偏移，避免怪物堆叠生成
- 对外暴露 `WaveStarted` / `WaveCompleted` / `WaveProgressChanged` 事件，UI 与波次逻辑解耦

### 玩家与商店
- 玩家拆分为两套正交状态机：移动状态（Idle / Walking / Running）与动作状态（Normal / Reloading / Dead）
- 开火打断奔跑并锁定至松开 Shift；死亡状态不可逆
- 击杀掉落金币，商店支持武器解锁购买与付费补弹，购买行按金币实时刷新可交互状态
- 打开商店时释放鼠标并挂起玩家控制器，避免 UI 操作穿透到角色控制

## 代码结构

```
Assets/
├── C#Script/
│   ├── 对象池/          ObjectPoolManager、PooledObject、IPoolable
│   ├── 武器系统/        WeaponData、WeaponManager、WeaponBase、WeaponUI、WeaponAmmoState
│   ├── 怪物系统/        Enembase（状态机）、EnemyData、spiderScript、WaveData、
│   │                    WaveSpawnPoint、WaveEnemyTracker
│   ├── 商店系统/        ShopUI、ShowRowUI
│   └── GameManager.cs、PlayerControlScript.cs、BulletControl.cs、recoilControl.cs
├── 武器数据/            AK47、M4、M107（ScriptableObject）
├── 怪物数据/            普通僵尸、Spider（ScriptableObject）
└── CSAssets2026/Scenes/ CityNew.unity（主场景）
```

## 如何运行

1. 使用 **Unity 2022.3.51f1c1** 或同系列 2022.3 LTS 版本打开本项目
2. 打开场景 `Assets/CSAssets2026/Scenes/CityNew.unity`
3. 点击运行

### 操作说明

| 按键 | 功能 |
|---|---|
| `W A S D` | 移动 |
| `Shift` | 奔跑 |
| `Space` | 跳跃 |
| 鼠标左键 | 开火 |
| 鼠标右键 | 瞄准 |
| `R` | 换弹 |
| `1` / `2` / `3` | 切换武器 |
| `B` | 打开/关闭商店 |

## 关于素材

本仓库为控制体积，**未包含以下第三方素材包**，克隆后需自行从 Asset Store 导入到对应路径：

| 素材包 | 应放置路径 |
|---|---|
| Infima Games - Low Poly Shooter Pack (Free Sample) | `Assets/CSAssets2026/Infima Games/` |
| RPG_FPS_game_assets_industrial | `Assets/` |
| Low Poly Buildings | `Assets/CSAssets2026/LowPolyBuildings/` |

> 缺少这些素材时，场景中的部分模型、材质与特效会显示为丢失状态，但不影响脚本逻辑与代码阅读。

## 已知不足与后续计划

- 命中特效仍走 `Instantiate` / `Destroy`，尚未纳入对象池
- 音效播放分散在各个武器与爆炸脚本中，尚无统一的全局音频管理器
- 波次系统没有同屏怪物数量硬上限，目前仅靠对象池容量兜底
- 尚无部位差异化伤害（爆头倍率），伤害在命中时统一结算

## 演示视频

<!-- 录制完成后把 B 站链接填在这里 -->
待补充
