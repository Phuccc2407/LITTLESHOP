# Khách hàng, đánh giá và âm thanh

## Bản cập nhật: lịch, cửa kết thúc ngày, màn hình và nhạc nền

- Scene chơi: `Assets/PolygonShops/Scenes/Demo_01.unity`. Build Settings đã chọn scene này làm scene bắt đầu. SampleScene được giữ lại nhưng không đưa vào build.
- Đã gắn `EndDayDoor` vào cửa `SM_Bld_Wall_Glass_Door_Double_01_R`. Nhìn vào cửa và bấm E để kết thúc ngày sớm: dừng spawn, kết thúc lượt khách, chốt Revenue / Expenses / Profit, hiện bảng tổng kết và chuyển sang 09:00 hôm sau. Đóng bảng tổng kết rồi mở cửa hàng bằng bảng LED khi sẵn sàng. Kết thúc ngày nhiều lần trong khi bảng đang mở bị chặn.
- Lịch bắt đầu **Thursday, 01/01/2026**, tự tính thứ và chuyển tháng/năm. HUD và bảng tổng kết dùng ngày thật; bộ đếm nội bộ vẫn giữ để tương thích code cũ.
- `ShopDayLighting` trên DayNightCycle tự tìm Directional Light, đổi intensity từ 0.03 ban đêm đến 1.2 buổi trưa, đổi màu và hướng nắng theo giờ. Có thể chỉnh Noon Intensity, Night Intensity hoặc tắt Rotate Sun trong Inspector. Ánh sáng gián tiếp đã bake vẫn phụ thuộc dữ liệu bake của scene.
- `AudioManager` đã có trong scene, gồm playlist nhạc nền, ambience và các mức Master / Music / Effects / Ambience. Không gán nhạc vẫn có một đoạn nhạc điện tử mẫu được tạo khi chạy. Gán Music Playlist và Ambience để thay bằng audio của bạn. `ShopAudio` trên cùng object xử lý tiếng sự kiện. Lời nhắc khi nhìn vật tương tác không phát tiếng.
- Bán coin: ô nhập luôn trống khi mở quầy, người chơi phải nhập đúng số coin khách yêu cầu. Số lượng yêu cầu vẫn hiện trong thông tin khách.
- `ComputerPanel/CustomerTabs` và `ComputerPanel/ReviewsPanel` đã được lưu trong scene. Mở máy tính rồi chọn **Customer Reviews**. ReviewsPanel mặc định tắt và chỉ bật khi chọn tab; phần cuộn được khởi tạo lúc chơi.
- Năm prefab máy đang dùng trong catalog đã có `ArcadeMachineNavigation`, `ArcadeMachineMedia` và con `ArcadeScreen`. Navigation tự tạo NavMeshObstacle với Carving theo kích thước model; preview đang di chuyển không cản NavMesh và không nhận khách. Máy đang có khách hoặc được giữ chỗ không được di chuyển.
- NPC không còn bị teleport vào PlayPoint trong thân máy. NPC chọn điểm tiếp cận trên NavMesh, tự tính lại đường và dùng avoidance chất lượng cao. Cần giữ lối đi đủ rộng cho bán kính NavMeshAgent.

### Gán ảnh, video và âm thanh cho từng máy

1. Mở asset trong `Assets/ScriptableObjects/Arcade`.
2. Trong **Screen and nearby audio**, gán Gameplay Video (VideoClip), hoặc một/nhiều Gameplay Images (Texture2D). Video có ưu tiên cao hơn ảnh; Image Interval quyết định thời gian đổi ảnh. Idle Screen là ảnh khi không có khách.
3. Gán Gameplay Audio nếu muốn loop riêng. Nếu có audio riêng, âm thanh trong video bị tắt để tránh phát chồng. Không gán clip vẫn có tiếng game mẫu khi chơi.
4. Trong prefab, `ArcadeMachineMedia/Screen Renderers` đã trỏ tới `ArcadeScreen`. Có thể đổi thành một mesh màn hình riêng của bạn. Không kéo renderer của toàn bộ máy nếu máy dùng chung atlas texture, vì nó sẽ đổi texture cả thân máy.
5. `ArcadeScreen` được đặt gần mặt trước máy theo collider làm vị trí ban đầu; chỉnh Local Position / Rotation / Scale để khớp chính xác màn hình của model, nhất là máy nhảy và bóng rổ. Material `Assets/Materials/ArcadeScreen.mat` dùng URP Unlit.
6. Khi NPC chơi: ảnh/video và âm thanh bắt đầu. Khi khách dừng hoặc máy bị tắt: video/audio dừng và trở về Idle Screen. Audio 3D giảm dần theo khoảng cách, mặc định hết trong khoảng 8m; chỉnh Volume / Audible Distance trên ArcadeMachineMedia.

### Kiểm tra bản cập nhật

- Ô coin: mở quầy hai lần, cả hai lần đều phải trống; nhìn vật tương tác không được có click.
- Reviews: mở máy tính, chọn Customer Reviews, kiểm tra vẫn thấy thông báo khi chưa có đánh giá; sau khi khách rời đi kiểm tra danh sách và cuộn.
- Đường đi: đặt một máy giữa lối, đợi carving cập nhật và xem khách đi vòng; kiểm tra lại sau khi reposition. Bảo đảm cửa và điểm quầy có đường NavMesh hợp lệ.
- Ngày: bấm E ở cửa khi đang mở hoặc đóng cửa hàng, kiểm tra ngày trong bảng tổng kết là ngày vừa kết thúc, HUD đã sang ngày mới, số liệu không cộng lại hai lần. Tới giờ đóng cửa tự động dùng cùng luồng này.
- Media: cho NPC chơi từng máy, đến gần/đi xa để nghe mức âm lượng; thử ảnh, video có âm thanh, video không âm thanh, rồi kết thúc ngày để chắc chắn tất cả máy dừng.
- Có menu `Little Shop > Install shop upgrades` để tạo lại/cập nhật cấu hình qua Unity và `Little Shop > Check shop upgrades` để kiểm tra lịch cùng các tham chiếu. Các script game và Editor đã được biên dịch riêng; kiểm thử bằng Unity batch mode chưa chạy được do licensing client của môi trường từ chối kết nối.

## Dùng ngay trong scene hiện tại

- Giữ CustomerManager, CoinCounter, các queuePoints và NavMesh đã có. Không cần đổi prefab hay sửa scene để bật các tính năng mới.
- Mở cửa: thời gian giữa các khách thay đổi ngẫu nhiên; đánh giá tốt giúp khách đến nhanh hơn. Max Customers vẫn giới hạn tổng khách trong scene.
- Khách chọn ngẫu nhiên một máy khả dụng, có ngân sách, số lượt chơi và thời gian kiên nhẫn riêng. Khách vẫn vào khi hết máy, sau đó rời đi và có thể đánh giá thiếu máy.
- Tại quầy, bán đúng số coin được yêu cầu. Giá bán dùng Cash Per Coin của EconomyManager. Giá máy dùng Coins Per Play của từng máy.
- Khách trừ đúng giá máy cho mỗi lượt, đổi sang máy khả dụng trong số coin còn lại và chỉ tăng Customers Served khi đã hoàn thành ít nhất một lượt. Coin chưa dùng hết không được hoàn lại.
- Chờ quầy lâu làm giảm hài lòng; hết kiên nhẫn khách bỏ về. Chơi giúp hồi hài lòng, máy xuống cấp và giá cao ảnh hưởng trải nghiệm.
- Mở ComputerPanel: hai nút Catalog / Reviews được tạo tự động. Reviews có cuộn, số sao trung bình, lời nhận xét và số lượt đã chơi.
- Lưu tối đa 100 đánh giá gần nhất bằng PlayerPrefs, khóa LittleShop.CustomerReviews.v1. Đây là lưu đánh giá; tiền, máy và ngày chơi vẫn theo hệ thống hiện có.

## Tùy chỉnh và mở rộng

1. Create > Little Shop > Customer Profile để tạo các nhóm khách. Ví dụ Casual: 1–2 lượt, ngân sách 6, kiên nhẫn 60s; Enthusiast: 3–6 lượt, ngân sách 20, kiên nhẫn 90s; Impatient: 1–3 lượt, ngân sách 8, kiên nhẫn 25s.
2. Kéo các asset vào CustomerManager > Customer Profiles. Để trống vẫn có thông số ngẫu nhiên mặc định. Price Sensitivity điều chỉnh tác động giá, Review Chance điều chỉnh xác suất viết đánh giá.
3. CustomerManager.CustomerSpawned / CustomerFinished là các event để nối nhiệm vụ, thống kê hoặc khách VIP. Customer.State, CompletedPlays, Satisfaction và PatienceRemaining phục vụ UI và analytics.
4. CustomerReviews.Changed dùng cập nhật UI; dữ liệu đánh giá không phụ thuộc ComputerPanel. CustomerReviews.Add có thể được gọi từ các loại khách mới.

## Âm thanh

- Có tiếng mở UI, khách đến, giao dịch coin, bắt đầu/kết thúc game, bực bội, đặt hàng, sửa máy, cửa và mở thùng.
- Nếu chưa gán clip, ShopAudio tự tạo tiếng điện tử ngắn để game có phản hồi ngay. AudioManager xử lý nhạc nền; ArcadeMachineMedia xử lý âm thanh máy trong không gian 3D. Chưa có giọng khách hay tiếng bước chân.
- Để dùng audio riêng, tạo một GameObject với ShopAudio trong scene. Trong Sounds, chọn Sound và thêm Clips; mỗi sự kiện chọn một clip ngẫu nhiên. Volume và Master Volume điều chỉnh âm lượng. Bảo đảm scene có AudioListener hoạt động.

## Kiểm tra trong Play Mode

1. Mở cửa với ít nhất 2 máy: quan sát thời gian spawn, số coin khách yêu cầu và việc chơi nhiều lượt/đổi máy.
2. Bỏ mặc quầy: khách hết kiên nhẫn phải rời hàng, máy được nhả đặt chỗ, khách phía sau tiến lên.
3. Mở cửa sổ bán coin rồi đợi khách bỏ đi: cửa sổ tự đóng; không bán nhầm cho người kế tiếp.
4. Thử nhập ít/nhiều coin hơn yêu cầu: không thu tiền. Bán đúng: tiền tăng đúng số coin nhân Cash Per Coin.
5. Đổi giá hoặc tắt máy khi khách đang đi tới: khách không bắt đầu game sai giá, nhả máy và để lại phản hồi.
6. Cho máy hết condition: máy không nhận khách mới; sửa máy có âm thanh và nhận khách lại.
7. Xem Reviews, đóng và mở lại game: đánh giá còn lưu, bảng cuộn được; quay lại Catalog vẫn đặt hàng được.
8. Kiểm tra âm thanh với AudioListener và thử gán clip vào ShopAudio để thay tiếng mặc định.

## Hướng nâng cấp tiếp theo sau khi chơi thử cân bằng

- Lưu toàn bộ tiến độ: tiền, máy đặt trong phòng, ngày chơi, đơn hàng đang giao.
- Khách có sở thích thể loại máy và khách quen mang số coin còn lại từ lần trước.
- Hệ thống vệ sinh/rác và nhân viên thu ngân, bảo trì để tạo lựa chọn đầu tư.
- Nhiệm vụ theo ngày và thống kê máy: lượt chơi, doanh thu, tỷ lệ sử dụng, chi phí sửa.
- Tiếng bước chân và chuyển routing âm thanh sang AudioMixer có hiệu ứng/ducking.

Các mục cuối là đề xuất, chưa triển khai trong thay đổi này.
