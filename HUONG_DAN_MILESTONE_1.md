# Milestone 1 - Hướng dẫn cài đặt và kiểm thử LITTLE SHOP

Hướng dẫn này thiết lập **Milestone 1** trong Unity Editor từ scene trống. Làm đúng theo thứ tự bên dưới; không bỏ qua bước tạo reference trong Inspector.

## 1. Kết quả sau khi setup

Khi hoàn tất, scene phải chạy được luồng sau:

1. Player xuất hiện trên sàn.
2. `WASD` di chuyển, chuột xoay camera, `Shift` chạy nhanh, `Space` nhảy.
3. Nhìn vào computer, prompt `E - Use computer` xuất hiện.
4. Nhấn `E`, Computer UI mở và chuột được mở khóa.
5. Nhấn `Buy`, tiền bị trừ một lần.
6. Sau thời gian giao hàng, một box xuất hiện tại điểm giao.
7. Box xuất hiện dưới dạng hộp giao hàng.
8. Nhìn vào box và nhấn `E` để mở; machine vào placement ngay.
9. Nhìn xuống sàn, chuột trái đặt, `R` xoay, chuột phải hoặc `Escape` hủy.
10. Nhìn vào machine đã đặt, nhấn `E`, Machine Settings UI mở.

## 2. Cấu trúc script

Tất cả script nằm bên trong `Assets/Scripts`:

```text
Assets/Scripts
├── Core
│   ├── ArcadeMachineDefinition.cs
│   ├── EconomyManager.cs
│   ├── GameViewManager.cs
│   ├── IInteractable.cs
│   ├── InventoryManager.cs
│   ├── ItemDefinition.cs
│   └── View.cs
├── Gameplay
│   ├── ArcadeMachine.cs
│   ├── Customer.cs
│   ├── CustomerDoor.cs
│   ├── CustomerManager.cs
│   ├── ComputerTerminal.cs
│   ├── DeliveryBox.cs
│   ├── DeliveryManager.cs
│   └── PlacementSystem.cs
├── Player
│   ├── CharacterController.cs
│   └── PlayerInteractor.cs
└── UI
    ├── CashierView.cs
    ├── ComputerView.cs
    ├── InteractTextView.cs
    ├── InventoryView.cs
    ├── MachineSettingsView.cs
    ├── NotificationView.cs
    ├── PauseView.cs
    ├── PlacementView.cs
    ├── ShopCatalogView.cs
    └── ShopManagementView.cs
```

`CharacterController.cs` trong `Assets/Scripts/Player` là script của project. Unity `CharacterController` là component built-in khác; Player cần gắn **cả hai**.

## 3. Chuẩn bị project

### 3.1 Mở đúng Unity version

Project hiện dùng Unity `6000.5.10f1`. Mở Unity Hub, chọn đúng project `D:/Unity/Projects/LITTLESHOP`.

Requirement gốc ghi Unity 2022.3, nhưng project thực tế đã được tạo bằng Unity 6 và dùng package version mới. Không mở bằng 2022.3 nếu chưa migrate package.

### 3.2 Kiểm tra package Input System

1. Chọn `Window > Package Manager`.
2. Ở dropdown phía trên, chọn `In Project`.
3. Tìm `Input System`.
4. Đảm bảo package đang ở trạng thái Installed.
5. Project hiện có `com.unity.inputsystem` version `1.20.0`.

Nếu Unity hỏi đổi active input handling:

1. Chọn `Edit > Project Settings`.
2. Chọn `Player`.
3. Mở `Other Settings`.
4. Tìm `Active Input Handling`.
5. Chọn `Input System Package (New)` hoặc `Both`.
6. Nếu Unity yêu cầu restart Editor, chọn restart.

### 3.3 Kiểm tra Input Actions

1. Trong Project window mở `Assets/InputSystem_Actions.inputactions`.
2. Trong cửa sổ Input Actions, chọn action map `Player`.
3. Kiểm tra các action sau:

| Action | Loại | Dùng cho |
|---|---|---|
| `Move` | `Value / Vector2` | WASD, arrow keys, gamepad left stick |
| `Look` | `Value / Vector2` | mouse delta |
| `Interact` | `Button` | phím E |
| `Jump` | `Button` | Space |
| `Sprint` | `Button` | Left Shift |

Không đổi tên map `Player` hoặc các action trên. Script tìm đúng các tên này khi chạy.

## 4. Tạo scene test và hierarchy

Có thể dùng `Assets/Scenes/SampleScene.unity`, hoặc tạo scene riêng:

1. Chọn `File > New Scene`.
2. Chọn template `Basic (Built-in)` hoặc scene 3D trống.
3. Chọn `File > Save As`.
4. Lưu thành `Assets/Scenes/Milestone1.unity`.

Hierarchy sau khi setup nên gần giống:

```text
Milestone1
├── Environment
│   ├── ShopFloor
│   ├── Wall_Back
│   ├── Wall_Left
│   ├── Wall_Right
│   ├── ComputerTerminal
│   └── DeliverySpawnPoint
├── Player
│   ├── PlayerCamera
│   └── (CharacterController + CharacterController.cs + PlayerInteractor)
├── GameSystems
│   ├── EconomyManager
│   ├── InventoryManager
│   ├── DeliveryManager
│   └── PlacementSystem
├── UIManager
│   └── Canvas
│       ├── InteractPrompt
│       ├── ComputerPanel
│       ├── InventoryPanel
│       └── MachineSettingsPanel
└── Directional Light
```

Tên object không bắt buộc đối với code, nhưng nên dùng đúng tên để dễ kiểm tra reference.

## 5. Tạo layer

### 5.1 Mở danh sách layer

1. Chọn bất kỳ GameObject trong Hierarchy.
2. Ở Inspector, mở dropdown `Layer`.
3. Chọn `Add Layer...`.

Tạo các layer:

```text
ShopFloor
PlacementObstacle
Interactable
Player
```

Layer mặc định `Default` vẫn giữ nguyên.

### 5.2 Quy tắc gán layer

- Mặt sàn: `ShopFloor`.
- Tường, quầy, đồ vật cản placement: `PlacementObstacle`.
- Computer và các object người chơi được raycast để tương tác: `Interactable`.
- Player: `Player`.
- Máy arcade sau khi đặt: nên ở `PlacementObstacle` để máy khác không chồng lên.
- Delivery box: gán `Interactable`.

## 6. Tạo môi trường test

### 6.1 Tạo parent Environment

1. Trong Hierarchy, click phải.
2. Chọn `Create Empty`.
3. Đặt tên `Environment`.
4. Reset Transform bằng menu dấu ba chấm trong Inspector > `Reset`.

### 6.2 Tạo ShopFloor

1. Chọn `Environment`.
2. Chọn `GameObject > 3D Object > Cube`.
3. Đặt tên `ShopFloor`.
4. Trong Transform:
   - Position: `(0, -0.1, 0)`.
   - Scale: `(12, 0.2, 12)`.
5. Giữ `Box Collider`.
6. Đặt Layer = `ShopFloor`.
7. Nếu Unity hỏi đổi layer cho child, chọn `Yes, change children` nếu object có child.

Mặt trên của cube ở khoảng `Y = 0`, vì vậy player có thể đứng ở `Y = 1`.

### 6.3 Tạo tường

Tạo ba Cube con của `Environment`:

| Tên | Position | Scale | Layer |
|---|---:|---:|---|
| `Wall_Back` | `(0, 1.5, 5.8)` | `(12, 3, 0.2)` | `PlacementObstacle` |
| `Wall_Left` | `(-5.8, 1.5, 0)` | `(0.2, 3, 12)` | `PlacementObstacle` |
| `Wall_Right` | `(5.8, 1.5, 0)` | `(0.2, 3, 12)` | `PlacementObstacle` |

Mỗi wall phải giữ `Box Collider`.

### 6.4 Tạo điểm spawn giao hàng

1. Click phải `Environment > Create Empty`.
2. Đặt tên `DeliverySpawnPoint`.
3. Set Position khoảng `(-3, 0.5, 3)`.
4. Rotation để `(0, 0, 0)`.

Đây là vị trí center của box khi giao. Đừng đặt điểm này bên trong tường hoặc dưới mặt sàn.

## 7. Tạo Player

### 7.1 Tạo object Player

1. Click phải ngoài `Environment`.
2. Chọn `Create Empty`.
3. Đặt tên `Player`.
4. Set Transform:
   - Position: `(0, 1, -3)`.
   - Rotation: `(0, 0, 0)`.
   - Scale: `(1, 1, 1)`.
5. Đặt Tag = `Player`.
6. Đặt Layer = `Player`.

### 7.2 Thêm built-in Character Controller

1. Chọn `Player`.
2. Inspector > `Add Component`.
3. Tìm `Character Controller` của Unity.
4. Thêm component.
5. Đặt:
   - Center: `(0, 0, 0)`.
   - Radius: `0.35`.
   - Height: `2`.
   - Slope Limit: `45`.
   - Step Offset: `0.3`.
   - Skin Width: `0.08`.
   - Min Move Distance: `0`.

Vì Player position đã ở `Y = 1`, collider cao 2 sẽ nằm trên mặt sàn `Y = 0`.

### 7.3 Thêm script CharacterController của project

1. Chọn `Player`.
2. `Add Component`.
3. Tìm script `CharacterController` trong `Assets/Scripts/Player`.
4. Nếu Unity hiện hai kết quả cùng tên, chọn script có icon C# nằm trong project, không chọn built-in component.
5. Trong component script, gán:

| Field | Giá trị |
|---|---|
| `Input Actions` | `Assets/InputSystem_Actions.inputactions` |
| `Camera Root` | kéo `PlayerCamera` vào |
| `Walk Speed` | `4` |
| `Sprint Speed` | `7` |
| `Mouse Sensitivity` | `0.1` |
| `Gravity` | `-20` |
| `Jump Height` | `1.2` |

Nếu field `Input Actions` để trống, Player sẽ không di chuyển và Console sẽ báo lỗi.

### 7.4 Tạo PlayerCamera

1. Chọn `Player`, click phải > `Camera`.
2. Đổi tên child thành `PlayerCamera`.
3. Set Local Transform:
   - Position: `(0, 0.8, 0)`.
   - Rotation: `(0, 0, 0)`.
   - Scale: `(1, 1, 1)`.
4. Đặt Tag = `MainCamera`.
5. Giữ component `Camera`:
   - Field of View: `75` hoặc `60`.
   - Clipping Near: `0.03`.
   - Clipping Far: `100`.
6. Kéo `PlayerCamera` từ Hierarchy vào field `Camera Root` của script project trên Player.

Camera phải là child của Player. Không đặt camera ngang hàng với Player, vì script xoay Player theo trục Y và xoay camera theo trục X.

### 7.5 HoldPoint không còn cần thiết

Box không tự mở khi được giao. Box chỉ hiển thị trong scene; người chơi phải nhìn vào box và nhấn `E`. Vì không pickup box nên không cần tạo `HoldPoint`.

### 7.6 Thêm PlayerInteractor

1. Chọn `Player`.
2. Add Component > `PlayerInteractor`.
3. Gán fields:

| Field | Giá trị |
|---|---|
| `Player Camera` | `PlayerCamera` |
| `Input Actions` | `Assets/InputSystem_Actions.inputactions` |
| `Interaction Range` | `3` |
| `Interaction Layers` | chỉ tick `Interactable` |
| `Interact Text View` | kéo `InteractPrompt` sau khi tạo UI |

`Interaction Layers` không được để `Everything`, nếu muốn xác nhận layer setup đúng. Raycast phải chạm Collider của object.

## 8. Tạo UI Canvas

### 8.1 Tạo Canvas

1. Click phải trong Hierarchy.
2. Chọn `UI > Canvas`.
3. Đặt tên `Canvas`.
4. Trong Canvas:
   - Render Mode: `Screen Space - Overlay`.
   - Pixel Perfect: có thể bật.
5. Unity sẽ tự tạo `EventSystem`. Nếu chưa có, tạo `UI > Event System`.

Nếu dùng TextMeshPro lần đầu, Unity có thể hỏi import TMP Essentials. Chọn `Import TMP Essentials`.

### 8.2 Tạo InteractPrompt

Inventory không còn nằm trong flow Milestone 1 đơn giản. Sau khi mở box, `DeliveryBox` gọi trực tiếp `PlacementSystem.BeginDirectPlacement`; không thêm item vào `InventoryManager`.

Nếu muốn giữ panel để phát triển về sau:

1. Chọn `Canvas`, click phải > `UI > Panel`.
2. Đặt tên `InteractPrompt`.
3. Xóa Image component nếu không muốn nền panel; giữ lại RectTransform.
4. Add Component > `Canvas Group`.
5. Add Component > script `InteractTextView` trong `Assets/Scripts/UI`.
6. Trong RectTransform:
   - Anchor Min: `(0.5, 0.15)`.
   - Anchor Max: `(0.5, 0.15)`.
   - Pivot: `(0.5, 0.5)`.
   - Width: `400`.
   - Height: `60`.
7. Tạo child `UI > Text - TextMeshPro`.
8. Đặt tên child `PromptText`.
9. Set text mẫu `INTERACTION`.
10. Chọn alignment giữa ngang và dọc.
11. Chọn parent `InteractPrompt`.
12. Trong `InteractTextView`, kéo `PromptText` vào field `Text`.

Script `View` sẽ tự tìm CanvasGroup trên cùng object. Không cần thêm CanvasGroup thứ hai.

### 8.3 Tạo ComputerPanel

1. Chọn `Canvas`, click phải > `UI > Panel`.
2. Đặt tên `ComputerPanel`.
3. Add Component > `Canvas Group`.
4. Add Component > `ComputerView`.
5. Set RectTransform full screen:
   - Anchor Min: `(0, 0)`.
   - Anchor Max: `(1, 1)`.
   - Left/Right/Top/Bottom: `0`.
6. Tạo các child sau:

```text
ComputerPanel
├── TitleText
├── ItemText
├── CashText
├── FeedbackText
├── BuyButton
├── OpenInventoryButton
└── CloseButton
```

Tất cả text phải là `Text - TextMeshPro`, tất cả button phải là `UI > Button - TextMeshPro`.

### 8.4 Cấu hình ComputerView cho nhiều mặt hàng

#### Gán catalog

Chọn object `ComputerPanel`, sau đó trong component `ComputerView`:

1. Tìm field `Catalog Items` và đặt `Size` bằng số máy arcade muốn bán.
2. Kéo từng asset `ArcadeMachineDefinition` vào các phần tử theo thứ tự:
   - `Element 0`: mặt hàng thứ nhất.
   - `Element 1`: mặt hàng thứ hai.
   - `Element 2`: mặt hàng thứ ba.
3. Gán `Delivery Manager`, `Item Text`, `Cash Text` và `Feedback Text`.
4. Khi mở Computer UI, mỗi máy sẽ xuất hiện thành một card có ảnh, tên, giá và
   nút `Mua` theo đúng thứ tự trên.

Tạm thời Computer UI chỉ bán `ArcadeMachineDefinition`, chưa bán các
`ItemDefinition` khác. Mỗi asset máy phải có `Prefab`, bật `Can Be Placed` và
bật `Can Be Unpacked`; nếu không, box có thể giao nhưng không thể tạo preview để
đặt máy.

#### Tạo khu vực card dạng grid

Không cần tạo thủ công từng card hoặc từng button. `ComputerView` sẽ tự tạo một
card cho mỗi máy trong `Catalog Items`; mỗi card gồm ảnh, tên, giá và nút `Mua`.

Trong `ComputerPanel`, tạo một child `UI > Empty` và đặt tên `ProductList`.
Chọn `ProductList`, thêm component `Content Size Fitter` với:

- `Horizontal Fit`: `Unconstrained`.
- `Vertical Fit`: `Preferred Size`.

Kéo `ProductList` vào field `Catalog Content` của `ComputerView`. Khi mở UI,
script sẽ tự thêm `GridLayoutGroup` với 3 cột và tự xếp các card theo dạng grid.

```text
ComputerPanel
├── ItemText (có thể tắt hoặc xóa)
├── ProductList (Catalog Content)
├── CashText
├── FeedbackText
├── OpenInventoryButton
└── CloseButton
```

Không gán `On Click()` thủ công cho các card. Nút `Mua` được tạo sẵn trong card và
gọi đúng sản phẩm tương ứng. Các hàm `PurchaseCatalogItem()` và
`PurchaseCatalogItem(int)` cũ vẫn được giữ để tương thích với button cũ nếu scene
đang có.

Chọn `OpenInventoryButton`:

1. `On Click () > +`.
2. Kéo `ComputerPanel` vào ô object.
3. Chọn `ComputerView > OpenInventory()`.

Chọn `CloseButton`:

1. `On Click () > +`.
2. Kéo `ComputerPanel` vào ô object.
3. Chọn `ComputerView > Close()`.

### 8.5 InventoryPanel (không dùng trong flow đơn giản)

1. Chọn `Canvas`, tạo `UI > Panel`.
2. Đặt tên `InventoryPanel`.
3. Add `Canvas Group`.
4. Add script `InventoryView`.
5. Tạo child:

```text
InventoryPanel
├── TitleText
├── InventoryInfoText
├── PlaceButton
└── CloseButton
```

6. Chọn `PlaceButton`, On Click kéo `InventoryPanel` vào và chọn `InventoryView > PlaceSelectedItem()`.
7. Chọn `CloseButton`, On Click kéo `InventoryPanel` vào và chọn `InventoryView > Close()`.

### 8.6 Tạo MachineSettingsPanel

1. Chọn `Canvas`, tạo `UI > Panel`.
2. Đặt tên `MachineSettingsPanel`.
3. Add `Canvas Group`.
4. Add script `MachineSettingsView`.
5. Tạo child:

```text
MachineSettingsPanel
├── StatusText
├── CoinsInput
├── ApplyButton
├── ToggleButton
├── RepairButton
├── RepositionButton
└── CloseButton
```

6. `CoinsInput` phải là `UI > Input Field - TextMeshPro`.
7. Chọn component `MachineSettingsView` trên panel:
   - Kéo `StatusText` vào `Status Text`.
   - Kéo `CoinsInput` vào `Coins Input`.
   - Kéo component `PlacementSystem` trên `GameSystems` vào `Placement System`.
8. Nối button:
   - `ApplyButton` → `MachineSettingsView.ApplySettings()`.
   - `ToggleButton` → `MachineSettingsView.ToggleMachine()`.
   - `RepairButton` → `MachineSettingsView.RepairMachine()`.
   - `RepositionButton` → `MachineSettingsView.RepositionMachine()`.
   - `CloseButton` → `MachineSettingsView.Close()`.

## 9. Tạo UIManager và đăng ký View

1. Click phải ngoài Canvas > `Create Empty`.
2. Đặt tên `UIManager`.
3. Add Component > `GameViewManager`.
4. Trong `All Views`, đặt `Size = 4`.
5. Kéo đúng bốn object vào list theo thứ tự bất kỳ:
   - `InteractPrompt`
   - `ComputerPanel`
   - `InventoryPanel`
   - `MachineSettingsPanel`
6. Để `Default View` trống.

Kiểm tra từng object:

| Object | Component bắt buộc | CanvasGroup |
|---|---|---|
| `InteractPrompt` | `InteractTextView` | có |
| `ComputerPanel` | `ComputerView` | có |
| `InventoryPanel` | `InventoryView` | có |
| `MachineSettingsPanel` | `MachineSettingsView` | có |

Không kéo `Canvas` parent vào `All Views`; phải kéo đúng object có script View.

## 10. Tạo GameSystems

### 10.1 Tạo object

1. Click phải ngoài Canvas > `Create Empty`.
2. Đặt tên `GameSystems`.
3. Add lần lượt:
   - `EconomyManager` từ `Assets/Scripts/Core`.
   - `InventoryManager` từ `Assets/Scripts/Core`.
   - `DeliveryManager` từ `Assets/Scripts/Gameplay`.
   - `PlacementSystem` từ `Assets/Scripts/Gameplay`.

### 10.2 EconomyManager

Chọn `GameSystems`, component `EconomyManager`:

- `Starting Cash = 10000`.

Không tạo thêm EconomyManager thứ hai trong scene. Script dùng singleton `EconomyManager.Instance`.

### 10.3 InventoryManager

Không cần gán field nào. Không chỉnh list runtime trong Inspector.

Item chỉ xuất hiện trong inventory sau khi box được mở thành công.

## 11. Tạo ArcadeMachineDefinition

### 11.1 Tạo asset

1. Trong Project window, mở thư mục `Assets`.
2. Có thể tạo thư mục `Assets/Data` để chứa data asset.
3. Click phải trong thư mục đó.
4. Chọn `Create > Little Shop > Arcade Machine Definition`.
5. Đặt tên asset `BasicArcadeMachine`.

### 11.2 Điền field

Chọn asset `BasicArcadeMachine`, Inspector sẽ có phần inherited từ `ItemDefinition` và phần machine:

| Field | Giá trị test | Ý nghĩa |
|---|---:|---|
| `Item Id` | `arcade_basic_01` | ID ổn định, không trùng item khác |
| `Display Name` | `Basic Arcade` | tên hiển thị |
| `Description` | `Máy arcade cơ bản` | mô tả |
| `Category` | `ArcadeMachine` | loại item |
| `Purchase Price` | `3000` | giá mua bằng yen |
| `Prefab` | kéo prefab máy vào | object spawn khi đặt |
| `Icon` | để trống | chưa bắt buộc ở prototype |
| `Placement Footprint` | `(1, 1, 1)` | vùng overlap dùng để kiểm tra |
| `Can Be Placed` | bật | cho phép placement |
| `Can Be Carried` | bật | cho phép luồng box/carry |
| `Can Be Unpacked` | bật | cho phép unpack |
| `Machine Type` | `Cabinet` | loại máy |
| `Coins Per Play` | `1` | dùng ở milestone sau |
| `Play Duration` | `30` | giây, dùng ở milestone sau |
| `Customer Capacity` | `1` | dùng ở milestone sau |
| `Attractiveness` | `1` | dùng ở milestone sau |
| `Maintenance Cost` | `100` | dùng ở milestone sau |

Trường `Prefab` cần gán sau khi tạo prefab ở bước 12. Không để trống.

## 12. Tạo prefab arcade machine

1. Trong Hierarchy, click phải `Environment > 3D Object > Cube`.
2. Đặt tên `BasicArcadeMachine`.
3. Set Transform:
   - Position: `(0, 1, 0)`.
   - Scale: `(1, 2, 1)`.
4. Add Component > script `ArcadeMachine` trong `Assets/Scripts/Gameplay`.
5. Giữ hoặc thêm `Box Collider`.
6. Đặt Layer = `PlacementObstacle`.
7. Kéo object từ Hierarchy vào `Assets/Prefabs` để tạo prefab.
8. Sau khi tạo prefab, xóa object `BasicArcadeMachine` khỏi scene; chỉ giữ prefab trong Project.
9. Chọn asset `BasicArcadeMachine` ở bước 11.
10. Kéo prefab từ Project vào field `Prefab`.
11. Mở prefab bằng double-click.
12. Đảm bảo component `ArcadeMachine` vẫn tồn tại trên root.
13. Lưu prefab.

Không đặt prefab này sẵn trong scene nếu muốn test đúng flow inventory → placement.

## 13. Tạo DeliveryBox prefab

### 13.1 Tạo box

1. Trong Hierarchy, click phải `Environment > 3D Object > Cube`.
2. Đặt tên `DeliveryBox`.
3. Set Scale `(0.8, 0.8, 0.8)`.
4. Add Component > `Rigidbody`.
5. Add Component > script `DeliveryBox` trong `Assets/Scripts/Gameplay`.
6. Giữ `Box Collider`.
7. Chọn component `DeliveryBox`, field `Body` kéo `Rigidbody` trên cùng object vào.
8. Đặt Layer = `Interactable`.
9. Kéo object vào `Assets/Prefabs` để tạo prefab.
10. Xóa object `DeliveryBox` khỏi scene sau khi đã tạo prefab.
11. Không cần kéo `PlacementSystem` vào prefab. `DeliveryManager` truyền `PlacementSystem` vào box tại runtime khi box giao đến.

Prefab phải có `Collider`, nếu không PlayerInteractor không thể raycast box.

### 13.2 Vì sao arcade machine cần interactable

Machine sau khi được đặt cần implement `IInteractable` thông qua component `ArcadeMachine`. Không cần thêm một component `Interactable` riêng.

Prefab machine phải có:

1. Collider bao phủ thân máy.
2. Component `ArcadeMachine`.
3. Layer nằm trong `PlayerInteractor.Interaction Layers`.
4. Layer nằm trong `PlacementSystem.Obstacle Layers` để máy khác không chồng lên.

Khi nhìn vào machine:

- Prompt: `E - Configure machine`.
- Nhấn `E`: mở `MachineSettingsView`.
- Trong UI có thể đổi giá, bật/tắt, sửa máy và reposition.

## 14. Tạo ComputerTerminal

1. Trong Hierarchy, click phải `Environment > 3D Object > Cube`.
2. Đặt tên `ComputerTerminal`.
3. Set Position khoảng `(3, 1, 2)`.
4. Set Scale khoảng `(1, 1.5, 0.6)`.
5. Giữ `Box Collider`.
6. Đặt Layer = `Interactable`.
7. Add Component > script `ComputerTerminal` trong `Assets/Scripts/Gameplay`.

Object này chỉ cần Collider và `ComputerTerminal`; không cần `Rigidbody`.

## 15. Cấu hình DeliveryManager

1. Chọn `GameSystems`.
2. Trong component `DeliveryManager`, gán:

| Field | Kéo object nào |
|---|---|
| `Delivery Spawn Point` | `Environment/DeliverySpawnPoint` |
| `Delivery Box Prefab` | prefab `DeliveryBox` trong Project |
| `Placement System` | component `PlacementSystem` trên `GameSystems` |
| `Delivery Duration` | `5` |

Nếu một trong ba reference đầu bị thiếu, mua hàng sẽ không tạo order và Console sẽ báo lỗi.

## 16. Cấu hình ComputerView

1. Chọn `ComputerPanel`.
2. Trong component `ComputerView`, gán:

| Field | Kéo object/asset nào |
|---|---|
| `Catalog Item` | asset `BasicArcadeMachine` |
| `Delivery Manager` | component `DeliveryManager` trên `GameSystems` |
| `Item Text` | child `ItemText` |
| `Cash Text` | child `CashText` |
| `Feedback Text` | child `FeedbackText` |

Không kéo `DeliveryManager` prefab hoặc script file; phải kéo component từ object `GameSystems`.

## 17. Cấu hình InventoryView và PlacementSystem

### 17.1 InventoryView

1. Chọn `InventoryPanel`.
2. Trong component `InventoryView`:
   - `Placement System`: kéo component `PlacementSystem` trên `GameSystems`.
   - `Item To Place`: asset `BasicArcadeMachine`.

### 17.2 PlacementSystem

1. Chọn `GameSystems`.
2. Trong component `PlacementSystem`, gán:

| Field | Giá trị |
|---|---|
| `Player Camera` | `Player/PlayerCamera` |
| `Floor Layers` | chỉ tick `ShopFloor` |
| `Obstacle Layers` | tick `PlacementObstacle` |
| `Grid Size` | `0.5` |
| `Placement Distance` | `5` |

Nếu muốn máy đã đặt chặn máy tiếp theo, máy phải ở layer `PlacementObstacle` trước khi đặt. Trong prototype, prefab máy đã có layer này.

Khi bắt đầu placement, script tắt collider của preview để preview không tự chặn raycast. Khi confirm, collider được bật lại.

## 18. Cấu hình MachineSettingsView

1. Chọn `MachineSettingsPanel`.
2. Trong component `MachineSettingsView`:
   - `Status Text`: kéo child `StatusText`.
   - `Coins Input`: kéo child `CoinsInput`.
3. Không cần gán machine bằng tay. Machine được truyền runtime khi player raycast vào object có `ArcadeMachine`.

Prefab machine phải có Collider và nằm trên layer được PlayerInteractor raycast nhìn thấy. Nếu để layer `PlacementObstacle` nhưng `Interaction Layers` chỉ chọn `Interactable`, player sẽ không tương tác được. Có hai cách:

- Cách đơn giản: gán máy sang layer `Interactable`, đồng thời chọn cả `Interactable` và `PlacementObstacle` trong `PlacementSystem.Obstacle Layers`.
- Cách dễ kiểm tra hơn: tạo layer `PlacedMachine`, chọn layer này trong cả `PlayerInteractor.Interaction Layers` và `PlacementSystem.Obstacle Layers`.

Khuyến nghị dùng cách thứ hai nếu project đã có nhiều vật cản.

## 19. Cấu hình PlayerInteractor và prompt

Quay lại `Player`:

| PlayerInteractor field | Reference |
|---|---|
| `Player Camera` | `PlayerCamera` |
| `Input Actions` | `InputSystem_Actions.inputactions` |
| `Interaction Range` | `3` |
| `Interaction Layers` | tick `Interactable` và layer máy đã đặt |
| `Interact Text View` | object `InteractPrompt` |

Nếu computer và box dùng layer `Interactable`, prompt sẽ hiện khi tâm camera nhìn đúng vào Collider trong phạm vi 3 units.

### 19.1 Cấu hình Quick Outline

Project đã có script Quick Outline tại `Assets/QuickOutline/Scripts/Outline.cs`.

Với mỗi object có thể tương tác:

1. Chọn root object của object đó, ví dụ `ComputerTerminal`, prefab `DeliveryBox` hoặc prefab arcade machine.
2. Chọn `Add Component > Outline`.
3. Trong component `Outline`:
   - `Outline Mode`: chọn `Outline Visible` hoặc `Outline All`.
   - `Outline Color`: chọn màu muốn dùng, ví dụ vàng nhạt.
   - `Outline Width`: đặt khoảng `4`.
4. Để component `Outline` **enabled** trong Inspector; `PlayerInteractor` sẽ tự tắt outline lúc không nhìn vào object.
5. Nếu Collider nằm trên child nhưng `Outline` nằm trên root, vẫn dùng được vì interactor tìm component bằng `GetComponentInParent`.

Khi tâm camera raycast trúng object interactable và object còn `CanInteract = true`, outline sẽ bật. Khi nhìn ra sàn, nhìn vào object không interactable hoặc ra khỏi `Interaction Range`, outline sẽ tắt và prompt cũng bị ẩn.


## 20. Kiểm tra toàn bộ reference trước khi Play

Trước khi bấm Play, mở từng object và đối chiếu:

### Player

- Built-in `CharacterController` có mặt.
- Script project `CharacterController` có `Input Actions`.
- Script project `CharacterController` có `Camera Root`.
- `PlayerInteractor` có camera, input asset và prompt.
- `PlayerCamera` có tag `MainCamera`.

### UIManager

- `GameViewManager` có 4 item trong `All Views`.
- Không có element `None`.
- Bốn object đều có CanvasGroup.

### GameSystems

- `EconomyManager` tồn tại đúng một bản.
- `InventoryManager` tồn tại đúng một bản.
- `DeliveryManager` có spawn point, box prefab, duration.
- `PlacementSystem` có camera, floor layer, obstacle layer.

### Assets

- `BasicArcadeMachine` có item ID, giá, footprint, prefab.
- Prefab máy có Collider và `ArcadeMachine`.
- Prefab box có Collider, Rigidbody, `DeliveryBox`.

### UI references

- `ComputerView` có item, delivery manager và ba TMP text.
- `InventoryView` có placement system và item.
- `MachineSettingsView` có status text và input field.
- Button events trỏ đúng component trên đúng panel.

## 21. Test theo từng chặng

### Test A - Player

1. Bấm Play.
2. Click vào Game view để khóa chuột.
3. Nhấn `W`, player phải tiến về phía trước.
4. Nhấn `A/D`, player phải sang trái/phải.
5. Di chuyển chuột, camera phải xoay.
6. Nhấn `Shift + W`, tốc độ phải tăng.
7. Nhấn `Space`, player phải nhảy.

Nếu fail, dừng test và sửa Player trước khi sang bước tiếp theo.

### Test B - Interaction Computer

1. Di chuyển đến trước computer.
2. Đặt tâm màn hình vào computer.
3. Prompt phải hiện `E - Use computer`.
4. Rời tâm khỏi computer; prompt phải ẩn.
5. Nhấn `E`; ComputerPanel phải hiện.
6. Cursor phải hiện và không còn khóa.
7. Bấm Close; panel ẩn và cursor bị khóa lại.

### Test C - Purchase và delivery

1. Mở computer.
2. Kiểm tra item text hiện `Basic Arcade` và giá `¥3,000`.
3. Ghi lại cash ban đầu, ví dụ `¥10,000`.
4. Bấm Buy đúng một lần.
5. Cash phải thành `¥7,000`.
6. Feedback phải báo đã đặt hàng.
7. Chờ 5 giây.
8. Box phải xuất hiện đúng tại `DeliverySpawnPoint`.

### Test D - Box và placement trực tiếp

1. Đóng ComputerPanel.
2. Nhìn đúng vào box.
3. Prompt phải là `E - Open box`.
4. Nhấn `E`; box mở và placement preview xuất hiện ngay.
5. Nhìn xuống sàn, nhấn `R` để xoay rồi nhấn chuột trái để đặt.
7. Không cần mở InventoryPanel.
8. Nhìn xuống sàn, nhấn `R` để xoay rồi nhấn chuột trái để đặt.
9. Nhấn chuột phải hoặc `Escape` để hủy; box được khôi phục để có thể mở lại.

Box chỉ chứa dữ liệu giao hàng khi `DeliveryManager` spawn. Người chơi phải nhìn vào box và nhấn `E`; lúc đó box mới mở và placement bắt đầu trực tiếp, không qua pickup hoặc Inventory.

### Test E - Placement

1. Không cần mở InventoryPanel; placement bắt đầu ngay sau khi box mở.
2. Nhìn xuống ShopFloor trong phạm vi 5 units.
3. Preview machine phải xuất hiện.
4. Nhấn `R`; preview xoay 90 độ.
5. Nhấn chuột trái tại sàn; machine được đặt.
6. Nếu nhấn chuột phải hoặc `Escape` trước khi đặt, box được khôi phục.

### Test F - Machine settings

1. Nhìn vào Collider của machine đã đặt.
2. Prompt phải hiện `E - Configure machine`.
3. Nhấn `E`.
4. MachineSettingsPanel phải hiện.
5. Nhập `2` vào CoinsInput.
6. Bấm Apply.
7. Status phải hiện `Price: 2 coin(s)`.
8. Bấm Toggle để đổi Inactive/Available.
9. Bấm Repair để đưa condition về 100%.

## 22. Lỗi thường gặp và cách sửa

### Console: `CharacterController requires an InputActionAsset`

Chọn `Player`, gán `Assets/InputSystem_Actions.inputactions` vào field `Input Actions` của script project. Đây không phải field của built-in CharacterController.

### Player không di chuyển

Kiểm tra theo thứ tự:

1. Project Settings đang dùng Input System.
2. Action map tên đúng `Player`.
3. Action tên đúng `Move`.
4. Input Actions đã gán vào script project.
5. Player có built-in CharacterController.
6. Player bắt đầu ở trên mặt sàn, không nằm trong tường.

### Không thấy prompt

Kiểm tra:

1. Camera có tag `MainCamera` không; field `PlayerInteractor.Player Camera` có gán camera không.
2. Object computer/box có Collider không.
3. Object có layer `Interactable` không.
4. `PlayerInteractor.Interaction Layers` có tick layer đó không.
5. `PlayerInteractor.Interact Text View` có kéo `InteractPrompt` không.
6. `InteractPrompt` có `InteractTextView`, CanvasGroup và TMP Text không.
7. Tâm camera có trỏ đúng Collider không.
8. Khoảng cách có nhỏ hơn `Interaction Range = 3` không.

### Nhấn E nhưng ComputerPanel không mở

1. `ComputerTerminal` có nằm trên object có Collider không.
2. `GameViewManager` có tồn tại không.
3. `ComputerPanel` có nằm trong `All Views` không.
4. `ComputerPanel` có CanvasGroup không.
5. Console có lỗi `View is not registered: ComputerView` không.

### Bấm Buy nhưng tiền không giảm

1. `EconomyManager` có trong scene không.
2. `Starting Cash` có lớn hơn `Purchase Price` không.
3. `ComputerView.Catalog Item` có gán asset không.
4. `ComputerView.Delivery Manager` có gán component từ `GameSystems` không.
5. Giá trong asset có đúng giá mong muốn không.

### Tiền bị trừ nhưng box không xuất hiện

Kiểm tra:

1. `Delivery Spawn Point` không None.
2. `Delivery Box Prefab` không None.
3. Prefab box có component `DeliveryBox` không.
4. Prefab box có Collider không.
5. Spawn point không nằm trong tường.
6. Xem Console ngay sau khi bấm Buy; lỗi giao hàng được log tại `DeliveryManager`.

### Box xuất hiện nhưng không mở được

1. Box có Collider.
2. Box layer được chọn trong `Interaction Layers`.
3. `DeliveryBox` có field `Body` trỏ tới Rigidbody.
4. `DeliveryManager` có `Placement System`.
5. Box đã được `Initialize` bởi DeliveryManager; không test bằng một object box đặt thủ công trong scene nếu chưa gọi Initialize.

### Nhấn E vào box nhưng không vào placement

Kiểm tra `DeliveryManager.PlacementSystem`, `BasicArcadeMachine.Prefab`, và `Can Be Placed` trong asset item.

### Không đặt được machine

1. `PlacementSystem.Player Camera` có gán không.
2. `Floor Layers` có tick `ShopFloor` không.
3. Camera có nhìn xuống đúng mặt sàn không.
4. Khoảng cách tới sàn có nhỏ hơn `Placement Distance` không.
5. `ItemDefinition.Prefab` có gán prefab không.
6. `ItemDefinition.Can Be Placed` có bật không.
7. `Placement Footprint` có quá lớn không.
8. Có obstacle trong vùng overlap không.

### Máy đặt được nhưng không tương tác được

Máy phải vừa nằm trong `PlacementSystem.Obstacle Layers` để chống overlap, vừa nằm trong `PlayerInteractor.Interaction Layers` để raycast tương tác. Chọn cả layer máy trong cả hai LayerMask.

## 23. Ghi chú giới hạn của Milestone 1

Milestone 1 hiện là prototype loop:

- Inventory chỉ lưu runtime, chưa save/load.
- Computer hỗ trợ nhiều máy arcade trong catalog; scene mẫu cần gán các
  `ArcadeMachineDefinition` vào `Catalog Items` và tạo button mua tương ứng.
- Inventory UI có một item chọn cố định để test placement.
- Box dùng một prefab đơn giản, chưa có animation mở.
- Placement preview lưu và khôi phục màu/alpha gốc của từng material, nên không
  làm kính hoặc vật liệu trong suốt của prefab bị trắng đục sau khi đặt.
- Chưa có cashier, NavMesh, coin sale hoàn chỉnh và shop day cycle.

Các hệ thống đó thuộc Milestone 2-4, không cần tạo object hoặc script của chúng để test Milestone 1.
