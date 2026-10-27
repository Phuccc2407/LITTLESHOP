# LITTLE SHOP
Người chơi sẽ quản lý 1 cửa hàng nhỏ, phục vụ khách hàng đến mua sắm, quản lý kho hàng và định giá sản phẩm để tối đa hóa lợi nhuận

Hoàng Bảo Phúc - 231230870

## Lộ trình triển khai

Project sử dụng Unity 6 và Input System mới. Các tính năng được triển khai theo từng giai đoạn để dễ kiểm thử:

1. **Giai đoạn 1 - Di chuyển nền tảng:** WASD/phím mũi tên và gamepad left stick, kèm trọng lực.
2. **Giai đoạn 2 - Camera:** camera góc nhìn người chơi và xoay bằng chuột/gamepad.
3. **Giai đoạn 3 - Tương tác:** phát hiện vật thể tương tác và hành động Interact.
4. **Giai đoạn 4 - Gameplay cửa hàng:** khách hàng, sản phẩm, kho hàng và định giá.
5. **Giai đoạn 5 - UI và hoàn thiện:** HUD, menu, âm thanh, cân bằng và tối ưu.

### Thiết lập Giai đoạn 1

1. Tạo một GameObject người chơi trong scene.
2. Thêm component Unity `CharacterController`.
3. Thêm script `CharacterController` trong `Assets/Scripts`.
4. Kéo `Assets/InputSystem_Actions.inputactions` vào trường **Input Actions**.
5. Đặt vị trí người chơi trên mặt sàn có collider rồi chạy scene.

Action map `Player` và action `Move` đã được cấu hình sẵn cho bàn phím và gamepad.

### Milestone 1 - Thiết lập các hệ thống

Tạo các asset bằng menu `Assets > Create > Little Shop`:

1. Tạo một `Arcade Machine Definition`, gán prefab máy có collider và component `ArcadeMachine`.
2. Tạo một GameObject hệ thống, thêm `EconomyManager`, `InventoryManager`, `DeliveryManager` và `PlacementSystem`.
3. Gán `DeliveryManager.deliverySpawnPoint` và prefab `DeliveryBox` có collider, Rigidbody và script `DeliveryBox`.
4. Gán `PlacementSystem.playerCamera`, `floorLayers`, `obstacleLayers` và `gridSize`.
5. Trên player, gán camera, camera root, Input Actions, `PlayerInteractor`, hold point và `InteractTextView`.
6. Thêm `GameViewManager` cùng các View vào `allViews`; mỗi View cần `CanvasGroup`.
7. Thêm `ComputerTerminal` vào máy tính và gán `ComputerView` với sản phẩm, `DeliveryManager` và các TMP fields.
8. Thêm `InventoryView`, gán `PlacementSystem` và item muốn đặt; nối nút UI với `PlaceSelectedItem`.
9. Gán `MachineSettingsView` vào `allViews`; nối các nút UI với `ApplySettings`, `ToggleMachine`, `RepairMachine`, `Close`.

Luồng kiểm thử Milestone 1:

`WASD + chuột` → nhìn vào máy tính và nhấn `E` → mua máy → chờ box xuất hiện → nhấn `E` để nhấc → nhấn `E` lần nữa để mở → gọi placement từ Inventory UI → di chuyển ghost, `R` xoay, chuột trái đặt, chuột phải/Escape hủy → nhìn vào máy và nhấn `E` để cấu hình.

Các View trống của các milestone sau đã được tách thành class riêng để không phá vỡ kiến trúc UI: cashier, shop catalog, shop management và pause sẽ được hoàn thiện ở Milestone 2-4.
