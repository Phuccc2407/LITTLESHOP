# Milestone 3 - Chu kỳ ngày và vận hành cửa hàng

Milestone 3 được triển khai theo từng phần. Phần đầu tiên thêm chu kỳ mở/đóng
cửa hàng; các phần thống kê, bảo trì máy và tổng kết ngày sẽ được bổ sung sau.

## 1. Chuẩn bị

1. Hoàn thành và kiểm thử [Milestone 2](./HUONG_DAN_MILESTONE_2.md).
2. Mở scene đang dùng cho Milestone 2.
3. Đảm bảo scene đã có `CustomerManager`, customer prefab, các điểm NavMesh
   và ít nhất một `ArcadeMachine` đang hoạt động.

## 2. Tạo ShopDayCycle

1. Tạo một Empty GameObject và đặt tên `ShopDayCycle`.
2. Thêm component `ShopDayCycle`.
3. Gán object `CustomerManager` hiện có vào field `Customer Manager`.
4. Chọn object `CustomerManager` và gán object `ShopDayCycle` vào field
   `Shop Day Cycle`.
5. Đặt các thông số:
   - `Day Length Minutes`: mặc định `10` phút ngoài đời cho đủ 24 giờ trong game.
   - `Opening Hour`: mặc định `9`, tương đương `09:00`.
   - `Closing Hour`: mặc định `22`, tương đương `22:00`.
6. Để `Starts Open` tắt để người chơi mở cửa bằng bảng LED.

Khi `ShopDayCycle` được gán vào `CustomerManager`:

- Shop ở trạng thái `Open` thì customer được spawn theo `Spawn Interval`.
- Gọi `CloseShop()` sẽ dừng spawn customer mới.
- Khi đóng thủ công, customer đã vào shop vẫn được phép hoàn thành lượt và đi ra.
- Đồng hồ 24 giờ luôn chạy kể cả khi shop đang đóng.
- Đến `Closing Hour`, shop bắt buộc chuyển sang `Closed` và dừng spawn customer
  mới; nếu vẫn còn customer trong shop, tất cả sẽ được yêu cầu xếp hàng rời
  shop qua `Exit Point`.
- Sau `Closing Hour`, thời gian tiếp tục chạy qua đêm; đến `00:00` mới tăng
  sang ngày tiếp theo.
- Từ `Opening Hour` của ngày mới, người chơi có thể mở ca bằng bảng LED.

## 3. Gắn bảng LED mở cửa

1. Chọn object bảng LED trong scene hoặc tạo một Empty GameObject ở vị trí bảng.
2. Thêm component `ShopLedBoard`.
3. Gán object `ShopDayCycle` vào field `Shop Day Cycle`.
4. Kéo các `Renderer` của bảng LED vào `Led Renderers`. Nếu để trống, component
   sẽ tự lấy toàn bộ Renderer trong object và các object con.
5. Kiểm tra material của bảng dùng shader có thuộc tính `_EmissionColor`.
6. Giữ `Open Emission Intensity = 4` và `Closed Emission Intensity = 0`.
7. Đảm bảo collider của bảng nằm trên `Interaction Layers` của
   `PlayerInteractor`.

Khi người chơi nhìn vào bảng và nhấn phím tương tác:

- Shop đang đóng: shop mở, customer bắt đầu được spawn và emission chuyển sang
  cường độ `4`.
- Shop đang mở: shop đóng, customer mới không spawn và emission chuyển về `0`.
- Nếu shop tự đóng lúc `Closing Hour`, emission cũng tự chuyển về `0`.
- Khi đang ngoài giờ mở cửa, bảng LED không thể mở shop và hiển thị giờ mở cửa.

## 4. Thêm UI trạng thái cửa hàng

1. Tạo một `Canvas` nếu scene chưa có Canvas dùng cho HUD.
2. Tạo hai TextMeshPro UI Text trong Canvas, đặt tên `ShopStateText` và
   `ShopTimeText`.
3. Tạo một Empty GameObject trong Canvas, đặt tên `ShopDayStatusHud`.
4. Thêm component `ShopDayStatusHud`.
5. Gán object `ShopDayCycle` vào field `Shop Day Cycle`.
6. Gán `ShopStateText` vào `State Text` và `ShopTimeText` vào `Time Text`.
7. Đặt vị trí hai text ở góc màn hình, không nằm trong CanvasGroup của view
   chặn gameplay.

HUD sẽ hiển thị `Shop: OPEN` hoặc `Shop: CLOSED` và đồng hồ 24 giờ theo dạng
`Day 1 - 09:00`. Khi shop đóng thủ công, đồng hồ vẫn tiếp tục chạy. Sau
`22:00`, đồng hồ tiếp tục qua `23:00`, `00:00` và các giờ ban đêm.

## 5. Kiểm thử phần đầu Milestone 3

1. Bấm Play và xác nhận bảng LED đang tắt.
2. Đi tới bảng, nhấn phím tương tác và xác nhận emission tăng lên `4`.
3. Xác nhận customer bắt đầu được spawn khi shop mở.
4. Kiểm tra HUD hiển thị `OPEN` và đồng hồ 24 giờ, ví dụ `Day 1 - 09:00`.
5. Tương tác bảng lần nữa và xác nhận emission về `0`, không có customer mới.
6. Kiểm tra HUD chuyển sang `CLOSED`.
7. Đặt `Day Length Minutes` rất ngắn, ví dụ `1` phút, để kiểm tra shop tự
   đóng lúc `22:00`, đồng hồ tiếp tục chạy qua đêm và tăng ngày lúc `00:00`.
8. Thử tương tác bảng LED trước `09:00` và xác nhận shop chưa thể mở.

## 6. Thêm thống kê customer vào HUD

1. Chọn object đang có component `ShopDayStatusHud`.
2. Kéo object `CustomerManager` vào field `Customer Manager`.
3. Tạo thêm một TextMeshPro UI Text tên `ShopCustomerText`.
4. Kéo `ShopCustomerText` vào field `Customer Text`.
5. Đặt text cạnh trạng thái shop và đồng hồ.

HUD sẽ hiển thị số customer đang ở trong shop và số customer đã hoàn tất lượt
trong ngày, ví dụ:

```text
Customers: 2
Entered today: 7
Served today: 5
```

`Entered today` tăng khi customer được spawn. `Served today` tăng khi customer
rời shop. Khi đồng hồ qua `00:00`, hai thống kê theo ngày được reset về `0`;
số customer đang còn trong scene vẫn được giữ cho đến khi họ hoàn tất và rời
shop.

## 7. Kiểm thử thống kê customer

1. Mở shop bằng bảng LED.
2. Xác nhận `Customers` tăng khi customer được spawn.
3. Chờ customer chơi xong và rời shop, xác nhận `Served today` tăng.
4. Đặt `Day Length Minutes` rất ngắn để đi qua `00:00`.
5. Xác nhận `Served today` reset về `0` khi sang ngày mới.

## 8. Cấu hình hao mòn và sửa máy

1. Chọn prefab hoặc object có component `ArcadeMachine`.
2. Đặt `Condition` ban đầu, thường là `100`.
3. Đặt `Condition Loss Per Play`, mặc định là `10`.
4. Mở `ArcadeMachineDefinition` tương ứng và đặt `Maintenance Cost`.
5. Đảm bảo scene có đúng một `EconomyManager`.
6. Mở `MachineSettingsView` từ máy arcade để xem condition và chi phí sửa.

Sau mỗi lượt chơi hoàn tất, condition của máy giảm theo `Condition Loss Per
Play`. Khi condition về `0`, máy chuyển sang `OutOfService` và customer mới
không chọn máy đó. Nút sửa chỉ hoạt động khi có đủ tiền; sửa thành công sẽ trừ
`Maintenance Cost` và đưa condition về `100`.

## 9. Kiểm thử hao mòn và sửa máy

1. Đặt `Condition Loss Per Play = 50` để test nhanh.
2. Mở shop và chờ customer hoàn tất một lượt.
3. Mở UI cấu hình máy, xác nhận condition giảm còn `50%`.
4. Chờ lượt tiếp theo, xác nhận condition về `0%` và state là `OutOfService`.
5. Xác nhận customer mới không chọn máy đã hỏng.
6. Nhấn nút sửa máy, xác nhận tiền mặt giảm đúng `Maintenance Cost`.
7. Xác nhận condition về `100%` và state trở lại `Available`.
8. Thử sửa khi không đủ tiền, xác nhận condition không thay đổi.

## 10. Tạo màn hình tổng kết cuối ngày

1. Tạo một UI Panel trong Canvas, đặt tên `ShopDaySummaryView`.
2. Thêm component `ShopDaySummaryView`.
3. Thêm một TextMeshPro UI Text tên `SummaryText` vào Panel.
4. Gán `ShopDayCycle`, `CustomerManager` và `SummaryText` vào component.
5. Thêm một Button `Close` và nối On Click tới
   `ShopDaySummaryView.Close()`.
6. Chọn object có `GameViewManager` và thêm `ShopDaySummaryView` vào danh sách
   `All Views`.
7. Không chọn Panel này làm `Default View`.

Khi đồng hồ đi qua `00:00`, màn hình tự hiện:

```text
Day 1 summary
Revenue: ¥500
Expenses: ¥100
Profit: ¥400
Customers served: 5
```

Doanh thu gồm tiền quy đổi từ coin ngay tại thời điểm player bán coin. Lượt
chơi chỉ làm máy kết thúc lượt và hao mòn, không cộng doanh thu lần nữa. Chi phí
gồm tiền mua máy và tiền sửa máy
trong ngày. Sau khi snapshot được hiển thị, thống kê doanh thu và chi phí ngày
được reset; tiền mặt tổng vẫn được giữ nguyên.

## 11. Kiểm thử tổng kết cuối ngày

1. Đặt `Day Length Minutes` ngắn để nhanh đi qua `00:00`.
2. Cho customer hoàn tất ít nhất một lượt chơi.
3. Sửa một máy để phát sinh chi phí.
4. Chờ đồng hồ qua `00:00`.
5. Xác nhận màn hình hiển thị doanh thu, chi phí, lợi nhuận và customer phục vụ.
6. Đóng màn hình, mở ca mới lúc `09:00`.
7. Xác nhận thống kê ngày mới bắt đầu lại từ `0`, còn `CurrentCash` vẫn giữ số
   dư sau ngày trước.

Nếu không muốn dùng chu kỳ ngày trong scene hiện tại, để trống field
`Shop Day Cycle` của `CustomerManager`. Hành vi spawn cũ sẽ được giữ nguyên.

Milestone 3 hoàn tất phần nền tảng khi chu kỳ ngày, customer, hao mòn, sửa máy
và tổng kết cuối ngày đều hoạt động đúng.

## 15. Cấu hình hàng đợi quầy coin

1. Tạo các Empty GameObject trước quầy theo thứ tự `CoinQueuePoint_0`,
   `CoinQueuePoint_1`, `CoinQueuePoint_2`...
2. Đặt các điểm cách nhau đủ rộng cho customer và nằm trên NavMesh.
3. Chọn object có `CoinCounter`, kéo các điểm vào mảng `Queue Points` theo thứ
   tự từ gần quầy nhất đến xa nhất.
4. Gán `CoinCounter` vào field `Coin Counter` của `CustomerManager`.
5. Đảm bảo `Coin Counter Point` là điểm đến chung trước quầy.

Customer đăng ký vào hàng ngay khi được spawn và đi thẳng tới vị trí chờ,
không đi qua quầy trước. Customer đến sau sẽ tự xếp vào vị trí tiếp theo.
Khi khách đầu tiên được bán coin, hàng tự dịch lên một vị trí và khách kế tiếp
trở thành người chờ giao dịch.

## 16. Tạo UI world-space cho customer

1. Mở prefab `Customer`.
2. Tạo child `Canvas`, đặt `Render Mode` là `World Space`.
3. Tạo một TextMeshPro UI Text làm child của Canvas.
4. Thêm component `CustomerWorldStatus` vào Canvas.
5. Gán TextMeshPro Text vào `Status Text`; để trống `Customer` để tự tìm parent.
6. Đặt Canvas trên đầu customer và chỉnh scale nhỏ, ví dụ `0.01`.

UI mặc định được ẩn. Khi tâm camera/crosshair của player raycast trúng một
customer, chỉ UI của customer đó mới hiện số coin cần mua và satisfaction,
đồng thời tự xoay về phía camera player:

```text
Coin: 2
Satisfaction: 87%
```

## 16.1. Đa dạng skin và outline customer

Prefab customer có field `Skin Prefabs`. Khi customer spawn, một prefab trong
mảng sẽ được chọn ngẫu nhiên. Có thể thay đổi danh sách này bằng các prefab
nhân vật humanoid dùng chung rig với animation customer.

1. Mở `Assets/Prefabs/Customers/C1.prefab`.
2. Trong component `Customer`, thêm các character prefab vào `Skin Prefabs`.
3. Đảm bảo mỗi skin có `Animator` và avatar tương thích với controller
   `Assets/Prefabs/Customers/Customer.controller`.
4. Chạy scene và xác nhận các customer mới có ngoại hình khác nhau nhưng vẫn
   dùng được `Walking`, `Standing Idle` và `Cheering`.

Customer có `CapsuleCollider` trên root và root nằm ở layer `Interactable`.
`PlayerInteractor` sẽ raycast từ tâm camera/crosshair; khi trúng customer,
outline màu vàng và UI status của đúng customer đó sẽ hiện. Khi crosshair rời
customer, outline và UI status sẽ tắt.

## 17. Tạo UI bán coin cho player

1. Tạo một UI Panel trong Canvas, đặt tên `CoinSaleView`.
2. Thêm component `CoinSaleView`.
3. Thêm ba TextMeshPro UI Text/Input:
   - `CustomerText`
   - `CoinInput`
   - `FeedbackText`
4. Gán các field tương ứng trong `CoinSaleView`.
5. Thêm Button `Sell` gọi `CoinSaleView.SellCoins()`.
6. Thêm Button `Close` gọi `CoinSaleView.Close()`.
7. Đăng ký `CoinSaleView` vào `GameViewManager.All Views`; không đặt làm
   `Default View`.
8. Chọn object quầy coin và đảm bảo collider của quầy nằm trong
   `PlayerInteractor.Interaction Layers`.

Khi player tương tác quầy, UI hiển thị khách đầu hàng và số coin cần bán.
Nhấn `Sell` với số coin hợp lệ sẽ cộng tiền ngay vào `EconomyManager`, sau đó
customer mới tiếp tục đi tới máy. Không cần chờ customer chơi xong để nhận
tiền bán coin; doanh thu lượt chơi vẫn được cộng khi lượt chơi hoàn tất.

## 18. Kiểm thử hàng đợi và bán coin

1. Mở shop và để ít nhất hai customer cùng tới quầy.
2. Xác nhận customer đứng lần lượt tại các `Queue Points`.
3. Nhìn lần lượt vào từng customer và xác nhận chỉ UI world-space của customer
   đang được ngắm hiển thị coin và satisfaction.
4. Tương tác quầy, nhập số coin nhỏ hơn yêu cầu và xác nhận giao dịch bị từ chối.
5. Nhập đủ coin rồi bấm `Sell`, xác nhận tiền mặt tăng ngay.
6. Xác nhận khách đầu tiên đi tới máy và khách kế tiếp tiến lên quầy.

## 12. Cấu hình độ hài lòng của customer

1. Chọn prefab `Customer`.
2. Đặt `Starting Satisfaction`, mặc định `100`.
3. Đặt `Satisfaction Loss Per Second`, mặc định `1`.
4. Chọn object có `ShopDayStatusHud`; HUD sẽ tự hiển thị mức hài lòng trung
   bình của các customer đang ở trong shop.

Customer mất hài lòng khi di chuyển và chờ phục vụ. Giá trị nằm trong khoảng
`0%` đến `100%`; giá trị này hiện tại dùng để theo dõi chất lượng phục vụ và
sẽ được dùng cho phần thưởng/phạt ở milestone sau.

## 13. Tạo quầy coin tương tác

1. Chọn object quầy coin trong scene hoặc tạo Empty GameObject tại quầy.
2. Thêm component `CoinCounter`.
3. Gán object quầy coin vào field `Coin Counter` của `CustomerManager`.
4. Đảm bảo `Coin Counter Point` là vị trí đứng trước quầy và nằm trên NavMesh.
5. Nếu quầy có Animator, tạo trigger `CustomerPurchase` và gán Animator vào
   field `Animator`.
6. Nếu muốn animation quầy, gán `Animator` và trigger `CustomerPurchase`.

Customer sẽ đi thẳng tới các `Queue Points` và xếp hàng chờ player bán coin.
Nếu không gán `CoinCounter`, flow cũ vẫn dùng vị trí quầy như một điểm đi qua
và nhận coin ngay lập tức.

## 14. Kiểm thử hài lòng và quầy coin

1. Mở shop và quan sát customer đi tới `Coin Counter Point`.
2. Xác nhận customer dừng lại xếp hàng trước quầy.
3. Xác nhận Animator quầy nhận trigger `CustomerPurchase` khi bán.
4. Xác nhận HUD hiển thị `Satisfaction: ...%`.
5. Đặt `Satisfaction Loss Per Second` thành `10` để thấy điểm giảm nhanh.
6. Bán đủ coin và xác nhận customer tiếp tục đi tới máy.

## 19. Cấu hình animation customer khi chờ

Script `Customer` điều khiển Animator bằng hai Bool Parameter:

- `IsWalking`: `true` khi customer đang đi tới queue, máy hoặc cửa ra.
- `IsPlaying`: `true` khi customer đang chơi máy.

Khi customer đứng chờ mua coin, script đặt cả hai parameter về `false`.
Animator Controller vì vậy phải có state đứng yên `Idle`.

1. Mở prefab customer, chọn component `Animator` và mở controller
   `Assets/Prefabs/Customers/Customer.controller`.
2. Trong tab `Parameters`, tạo hai parameter kiểu `Bool` đúng tên:
   `IsWalking` và `IsPlaying`.
3. Kéo animation đứng yên của model vào Animator Graph để tạo state `Idle`.
   Có thể dùng clip idle tương thích với rig customer; không dùng clip
   `Standing Idle` của player nếu khác avatar/rig.
4. Đặt `Idle` làm `Layer 0` default state.
5. Tạo các transition sau:

   - `Idle -> Walking`: Condition `IsWalking == true`.
   - `Walking -> Idle`: Condition `IsWalking == false`.
   - `Walking -> Cheering` (hoặc state chơi máy): Condition
     `IsPlaying == true`.
   - `Cheering -> Idle`: Condition `IsPlaying == false`.

6. Với các transition dùng Bool, bỏ `Has Exit Time` để animation đổi trạng
   thái ngay khi customer tới queue hoặc được bán coin. Đặt Transition
   Duration ngắn, khoảng `0.1` đến `0.25` giây.
7. Chọn state `Idle` và bật `Loop Time` cho clip idle. Clip `Walking` và
   `Cheering` cũng nên bật loop nếu animation được phát lặp.
8. Trong prefab customer, kiểm tra các field của `Customer`:
   - `Animator` trỏ tới Animator trên root customer.
   - `Walking Parameter` là `IsWalking`.
   - `Playing Parameter` là `IsPlaying`.

Luồng kiểm thử:

```text
Spawn -> IsWalking = true -> Walking
Tới queue -> IsWalking = false, IsPlaying = false -> Idle
Bán coin -> IsWalking = true -> Walking
Tới máy -> IsWalking = false, IsPlaying = true -> Cheering/Playing
Hết lượt -> IsWalking = true, IsPlaying = false -> Walking
```
