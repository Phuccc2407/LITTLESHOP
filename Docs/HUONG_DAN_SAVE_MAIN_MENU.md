# Main menu và lưu tiến độ

## Bắt đầu chơi

Mở `Assets/Scenes/MainMenu.unity` rồi bấm Play. Build Settings đã đưa MainMenu lên đầu, tiếp theo là scene gameplay `Assets/PolygonShops/Scenes/Demo_01.unity`.

Main menu được tạo sẵn trong scene bằng UGUI + TextMeshPro, chỉnh trực tiếp trong Hierarchy/Inspector, gồm:

- **Continue**: tiếp tục bản lưu. Nút bị khóa khi không có file lưu đọc được. Ngày và số tiền được hiển thị bên dưới.
- **New Game**: bắt đầu lại với cấu hình mặc định của scene, ngày Thursday, 01/01/2026. Nếu có bản lưu, game yêu cầu xác nhận trước khi thay thế.
- **Settings**: Master / Music / Sound Effects; dùng lại AudioManager và lưu cấu hình âm lượng riêng.
- **Quit**: thoát ứng dụng. Trong Unity Editor, Application.Quit không dừng Play Mode.

Tên game ở top-center, bốn nút Continue / New Game / Settings / Quit trải ngang ngay bên dưới. Canvas, camera, EventSystem, AudioManager, Settings và bảng xác nhận New Game đều đã lưu trong scene. MainMenuController chỉ cập nhật trạng thái và xử lý nút; không tạo UI khi chạy. Button On Click và Slider On Value Changed đã gắn sẵn, có thể xem/chỉnh trong Inspector.

### Gán ảnh nền và chỉnh giao diện thủ công

1. Import ảnh của bạn, đặt Texture Type = **Sprite (2D and UI)** rồi Apply.
2. Chọn **Canvas/Background**, kéo sprite vào **Image > Source Image**. MenuBackground và AspectRatioFitter tự phủ kín màn hình, giữ đúng tỷ lệ ảnh (có thể crop mép nếu tỷ lệ màn hình khác).
3. Image đã gắn material **Assets/Materials/MenuBackgroundBlur.mat**. Chọn material, chỉnh **Blur Radius**: 0 = nét, khoảng 3–8 = mờ. Không đưa background vào Sprite Atlas vì shader lấy mẫu quanh pixel của ảnh nền.
4. **Canvas/DarkOverlay > Image > Color > Alpha** chỉnh độ tối, mặc định 0.6. Đây là lớp tối riêng, không phải hiệu ứng blur.
5. **Canvas/GameTitle**: sửa text/font/màu/kích thước tên game. RectTransform anchor ở top-center.
6. **Canvas/MenuContent/ButtonRow**: di chuyển cả hàng nút hoặc chỉnh từng nút con và Label. Có thể chỉnh trực tiếp RectTransform, Image và TMP; script không đặt lại vị trí.
7. SettingsPanel và NewGameConfirmation mặc định tắt. Bật tạm trong Hierarchy để chỉnh các bảng con, tắt lại trước khi lưu scene. Các tham chiếu trên MainMenuController đã nối sẵn; giữ nguyên khi chỉ đổi ảnh/text/bố cục.

Shader blur ảnh nền đã được thêm vào project; phần shader và giao diện cần xem lại trong Unity Play Mode do môi trường kiểm thử tự động chưa mở được Unity vì licensing client.

## Lưu và tải

- Tự lưu mỗi 60 giây, sau khi kết thúc ngày, khi ứng dụng chuyển nền và khi thoát game.
- **F5**: lưu nhanh.
- **Esc**: nếu đang mở bảng UI thì đóng bảng đó; nếu không, mở menu tạm dừng. Menu có Resume, Save Game, Save & Main Menu, Save & Quit. Thời gian gameplay dừng khi menu tạm dừng mở.
- Khi đang đặt/di chuyển máy, hoàn tất hoặc hủy placement trước khi lưu. Nếu thoát trực tiếp, game hủy thao tác đặt dở trước khi thử lưu để trả lại đồ/thùng/vị trí cũ.
- Save & Main Menu và Save & Quit chỉ tiếp tục khi lưu thành công; lỗi sẽ hiện trên menu.
- Mở trực tiếp scene gameplay trong Editor sẽ tự tải tiến độ đã có. Muốn test từ đầu, chạy MainMenu và chọn New Game.

## Những gì được lưu

- Tiền hiện tại; tổng và doanh thu/chi phí trong ngày.
- Ngày, giờ, trạng thái mở cửa; số khách đã vào/được phục vụ trong ngày.
- Tất cả máy đã đặt: loại máy, vị trí, hướng, tỷ lệ, tình trạng, giá coin, bật/tắt và doanh thu.
- Số lượng đồ trong kho.
- Đơn đang giao cùng thời gian còn lại và thùng hàng chưa mở cùng vị trí.
- Vị trí/hướng player và góc nhìn dọc.
- Lịch sử đánh giá khách hàng.

Các lượt khách đang diễn ra không được khôi phục: khách sẽ vào lại theo hệ thống spawn, máy được giải phóng khi Continue. Tiền đã thu vẫn giữ nguyên. Đây là lưu tiến độ cửa hàng, không phải snapshot toàn bộ NPC/animation/video.

## File lưu và dự phòng

File nằm trong `Application.persistentDataPath`, tên **shop-save-v1.json**. Có thể xem đường dẫn bằng thuộc tính `GameSession.SavePath` trong code.

Ghi file qua `.tmp`, hoàn thành rồi thay thế file chính. `.bak` giữ bản lưu thành công trước đó và xoay vòng mỗi lần lưu. Nếu file chính không đọc được, game thử `.bak`. File chính hỏng được giữ với đuôi `.corrupt-...` khi ghi lại dữ liệu đã khôi phục. Không có cloud save hoặc nhiều slot trong phiên bản này.

Không dùng Clear All PlayerPrefs để xóa save: file tiến độ và PlayerPrefs là hai nơi riêng. Dùng New Game để khởi tạo lại tiến độ.

## Thêm item/máy mới

Mỗi ItemDefinition cần **Item ID duy nhất**, không đổi ID sau khi đã phát hành save. Chạy **Little Shop > Refresh save item catalog** để cập nhật `Assets/Resources/ShopItemCatalog.asset` sau khi thêm asset trong `Assets/ScriptableObjects`.

Máy khôi phục từ prefab trên ArcadeMachineDefinition, nên prefab cần ArcadeMachine tại root. Save được kiểm tra catalog trước khi thay thế đồ vật trong scene; thiếu item/prefab sẽ báo lỗi thay vì âm thầm bỏ mất đồ. GameSession/ShopSaveData chịu trách nhiệm điều phối và định dạng; các manager có CaptureProgress/RestoreProgress hoặc API tương đương để tiếp tục mở rộng.

## Kiểm tra nhanh trong Play Mode

1. New Game, kiểm tra tiền/ngày mặc định, mua một máy và lưu khi đơn còn đang giao.
2. Save & Main Menu rồi Continue: đơn giao tiếp tục, không bị trừ tiền lần nữa.
3. Đặt máy, đổi giá coin, tắt máy hoặc để condition giảm; lưu và Continue, kiểm tra số máy/vị trí/giá/condition giữ nguyên, không nhân đôi.
4. Lưu thùng chưa mở và đồ trong kho, rồi tải lại để kiểm tra số lượng.
5. Kết thúc ngày ở cửa: Continue phải ở ngày mới và không cộng lại doanh thu ngày cũ.
6. Thử New Game khi đã có tiến độ: Back giữ bản lưu; Start New Game thay thế bằng lượt mới.
7. Khi game đã đóng, sao lưu file rồi thử làm hỏng file chính; Continue phải đọc được bản `.bak` nếu bản dự phòng hợp lệ.

Đã kiểm tra riêng phần ghi file: ghi lần đầu, UTF-8, thay thế file, xoay backup, từ chối dữ liệu rỗng và dọn file tạm khi ghi lỗi. Biên dịch C# được kiểm tra bằng compiler của Unity. Kiểm thử Play Mode trong môi trường tự động vẫn bị giới hạn bởi lỗi Unity licensing client đã nêu trước đó.
