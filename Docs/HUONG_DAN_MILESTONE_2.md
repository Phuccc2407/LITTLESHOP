# Milestone 2 - Customer và vận hành cửa hàng

## Cấu hình customer

Đã hoàn thành bước nền tảng đầu tiên:

- Máy arcade có trạng thái `Occupied` khi đang có lượt chơi.
- Mỗi máy lưu doanh thu riêng trong runtime.
- Customer ở bước tiếp theo có thể gọi `TryStartPlay()`, `CompletePlay()` hoặc
  `CancelPlay()` trên `ArcadeMachine`.
- Khi lượt chơi hoàn tất, doanh thu được cộng vào `EconomyManager`.
- `MachineSettingsView` hiển thị doanh thu hiện tại của máy.

Tạo một prefab customer đơn giản:

1. Tạo `GameObject > 3D Object > Capsule`, đặt tên `Customer`.
2. Thêm component `Customer`, `NavMeshAgent` và tùy chọn `Animator`.
3. Kéo object này vào thư mục `Assets/Prefabs/Customers` để tạo prefab.
4. Bật package `AI Navigation`, sau đó tạo một Empty GameObject tên
   `ShopNavMeshSurface` và thêm component `NavMeshSurface`.
5. Cấu hình `NavMeshSurface` như sau:
   - `Collect Objects`: chọn `Volume`, không chọn `All`.
   - `Center` và `Size`: bao phủ **toàn bộ sàn shop, phần cửa ra vào và một đoạn
     hành lang bên ngoài**. Không chỉ bao phủ phần phòng bên trong.
   - `Use Geometry`: chọn `Render Meshes` nếu sàn dùng mesh; chọn
     `Physics Colliders` nếu sàn dùng collider.
   - `Layer Mask`: chỉ chọn layer sàn và các layer thật sự có thể đi trên đó.
6. Chọn `Bake`. Khi bật gizmo NavMesh, vùng đi được phải là một mảng xanh liên
   tục từ hành lang → cửa → bên trong shop.

Vì scene là một phòng trong trung tâm thương mại, không được bake riêng phần
phòng nếu muốn NPC đi từ mall vào shop. NPC sẽ chỉ đi được giữa các vùng có
NavMesh nối liền; cửa hoặc tường nằm giữa hai vùng sẽ làm path bị ngắt.

#### Kiểm tra cửa và vật cản

- Cửa ra vào phải nằm trên vùng NavMesh và phải có một khoảng trống đủ rộng cho
  `NavMeshAgent`.
- Không để collider của cánh cửa đóng kín vùng lối đi khi bake. Có thể bỏ layer
  cửa khỏi `Layer Mask` của `NavMeshSurface`, hoặc dùng `NavMeshObstacle` cho
  vật cản động và tắt `Carve` nếu cửa chỉ xoay trang trí.
- Không để tường, quầy hoặc collider của props phủ lên lối đi.
- Nếu dùng `NavMeshObstacle` có `Carve`, chỉ bật cho vật cản cố định; tránh bật
  carve trên cánh cửa đang animation vì vùng NavMesh có thể cập nhật không ổn
  định trong lúc cửa xoay.

#### Đặt điểm cho CustomerManager

Các điểm phải nằm trực tiếp trên vùng NavMesh màu xanh:

1. `CustomerEntrance`: đặt **ngoài cửa một đoạn ngắn**, trên hành lang mall,
   không đặt xuyên vào tường hoặc trên bậc thềm.
2. `CoinCounterPoint`: đặt bên trong shop, trước quầy và có khoảng trống cho
   agent dừng lại.
3. `CustomerExit`: đặt ngoài cửa trên cùng vùng NavMesh với
   `CustomerEntrance`.

Nếu muốn NPC đi vào được, cần có đường NavMesh liên tục giữa cả ba điểm. Không
đặt `CustomerEntrance` ở vùng mall và `CoinCounterPoint` trong phòng nếu cửa
đang làm hai vùng này bị tách rời.

#### Cách kiểm tra NPC đang đi vòng

1. Tạm dừng Play Mode.
2. Chọn `ShopNavMeshSurface` và bật hiển thị NavMesh.
3. Kiểm tra vùng xanh có đi xuyên qua đúng lối cửa hay không.
4. Chọn prefab Customer trong scene và xem `NavMeshAgent`:
   - Nếu `Is On NavMesh` là `false`, điểm spawn đang sai hoặc chưa bake.
   - Nếu `Path Pending` giữ ở `true`, NavMesh chưa được bake đúng hoặc surface
     không hoạt động.
   - Nếu path kết thúc trước cửa, vùng trong shop và hành lang đang bị ngăn cách.
5. Đặt `CustomerEntrance` ngay trên vùng xanh ngoài cửa, sau đó kéo dần vào
   trong để tìm vị trí mà agent có thể đi qua cửa.
6. Đặt `CoinCounterPoint` và `CustomerExit` trên vùng xanh; tránh đặt chúng
   lên collider của quầy, tường hoặc cánh cửa.

Có thể kiểm tra nhanh bằng cách đặt một `NavMeshAgent` thủ công lên hành lang,
bấm Play và gọi `Set Destination` tới một Empty trong shop. Nếu agent không tìm
được đường, cần sửa NavMesh trước khi kiểm tra script customer.

#### Cấu hình NavMeshAgent

Trên prefab `Customer`, chọn `NavMeshAgent`:

- `Radius`: nhỏ hơn một nửa bề rộng lối cửa; bắt đầu với `0.3`.
- `Height`: phù hợp chiều cao capsule; bắt đầu với `1.8`.
- `Base Offset`: thường là `0` nếu chân capsule nằm trên mặt sàn.
- `Stopping Distance`: khoảng `1.0` để dừng trước quầy/máy.
- `Speed`: bắt đầu với `2`.

Nếu radius lớn hơn cửa, agent có thể báo không có đường dù vùng NavMesh nhìn
liền nhau.

#### Hoàn tất cấu hình CustomerManager

7. Tạo các Empty GameObject trong scene:
   - `CustomerEntrance`: điểm đứng ngoài cửa.
   - `CoinCounterPoint`: điểm dừng tại quầy mua xu.
   - `CustomerExit`: điểm đứng ngoài cửa khi rời shop.
8. Tạo Empty GameObject `CustomerManager`, thêm component `CustomerManager`.
9. Gán `Customer Prefab`, `Entrance Point`, `Coin Counter Point` và `Exit Point`.
10. Có thể bắt đầu với `Max Customers = 3`, `Spawn Interval = 10`.
11. Bật `Spawning Enabled` để tự động spawn; tắt tùy chọn này và gọi
   `CustomerManager.SpawnCustomer()` từ một button nếu muốn test thủ công.

`CustomerManager` dùng `NavMesh.SamplePosition` tại `CustomerEntrance`; nếu
điểm này không nằm gần NavMesh, Console sẽ báo `Customer entrance is not on a
baked NavMesh`. Đây là lỗi cấu hình điểm hoặc bake, không phải lỗi prefab.

Customer dùng `NavMeshAgent` theo luồng:

1. Spawn ở `CustomerEntrance`.
2. Đi tới `CoinCounterPoint` và mua đủ số coin cho một lượt của máy.
3. Đi tới máy arcade đang `Available`.
4. Chơi theo `Play Duration` của máy.
5. Gọi `CompletePlay()`: tiền chơi được cộng vào `EconomyManager.CurrentCash`,
   tức tiền hiện có của player.
6. Đi tới `CustomerExit` rồi bị hủy khỏi scene.

`CustomerManager` giữ chỗ máy ngay khi spawn customer để nhiều customer không
cùng chọn một máy trong lúc đang di chuyển. Reservation được giải phóng nếu
customer không thể tới máy hoặc lượt chơi bị hủy.

#### Điểm đứng/chỗ chơi cố định cho từng máy

Điểm đứng của customer nên nằm trong prefab máy, không đặt chung trong
`CustomerManager`:

1. Mở prefab arcade machine.
2. Tạo child `Empty GameObject` và đặt tên `PlayPoint`.
3. Đặt `PlayPoint` đúng vị trí customer đứng/ngồi khi chơi.
4. Xoay trục `Y` của `PlayPoint` theo hướng mặt customer nhìn vào máy.
5. Chọn object có component `ArcadeMachine`, kéo child `PlayPoint` vào field
   `Play Point`.
6. Apply thay đổi vào prefab.

Nếu chưa gán `Play Point`, hệ thống dùng vị trí và hướng của chính machine làm
fallback. Khi tới nơi, customer sẽ snap vào `PlayPoint`, giữ máy ở trạng thái
`Occupied` và không còn đứng lệch quanh máy.

#### Coin và tiền mặt

Coin là đơn vị riêng, không dùng chung với tiền mua máy. Tỷ giá mặc định hiện
tại là **1 coin = ¥100**:

1. Chọn object `EconomyManager`.
2. Trong Inspector, chỉnh field `Cash Per Coin` nếu muốn đổi tỷ giá.
3. `Coins Per Play` của mỗi máy là số coin customer phải mua/trả cho một lượt.
4. Khi lượt chơi hoàn tất, máy ghi nhận số coin và quy đổi thành tiền mặt theo
   `Cash Per Coin`, sau đó cộng vào `EconomyManager.CurrentCash` của player.

Ví dụ: máy thu `2 coin(s)` và tỷ giá là `¥100/coin` thì player nhận `¥200`.
Giá mua máy trong Computer UI vẫn dùng tiền mặt và không bị ảnh hưởng bởi số
coin trên máy.

#### Mở rộng animation customer

`Customer` đã có hai hook Animator:

- `IsWalking` (`bool`): bật khi customer đang di chuyển.
- `IsPlaying` (`bool`): bật khi customer đang chơi máy.

Để dùng animation:

1. Thêm `Animator` vào customer prefab.
2. Tạo Animator Controller với parameter `IsWalking` và `IsPlaying`.
3. Tạo transition từ idle sang walk khi `IsWalking = true`.
4. Tạo state chơi/ngồi/đứng khi `IsPlaying = true`.
5. Kéo Animator vào field `Animator` của component `Customer`.

Có thể đổi tên parameter trong Inspector của `Customer` nếu Animator Controller
dùng tên khác. Sau này có thể thêm animation mua xu, xoay người hoặc animation
riêng cho từng loại máy mà không thay đổi flow NavMesh.

Để cửa mở xoay khi customer đi vào:

1. Chọn object cửa hoặc tạo object parent `CustomerDoor`.
2. Thêm component `CustomerDoor`.
3. Gán phần cánh cửa có thể xoay vào `Door Transform`.
4. Đặt `Open Euler Angles`, ví dụ `(0, 90, 0)`.
5. Kéo object cửa vào field `Entrance Door` của `CustomerManager`.

Manager mở cửa trước khi spawn customer. Cửa đóng sau khi customer đã đi ra.
Nếu không còn máy trống, customer sẽ không spawn ở lượt đó.

Đã hoàn thành bước nền tảng đầu tiên:

- Máy arcade có trạng thái `Occupied` khi đang có lượt chơi.
- Mỗi máy lưu doanh thu riêng trong runtime.
- Customer gọi `TryStartPlay()`, `CompletePlay()` hoặc `CancelPlay()` trên
  `ArcadeMachine`.
- Khi lượt chơi hoàn tất, doanh thu được cộng vào `EconomyManager`.
- `MachineSettingsView` hiển thị doanh thu hiện tại của máy.

Các bước tiếp theo là thêm UI số khách, quầy xu trực quan và bắt đầu triển khai
shop day cycle.
