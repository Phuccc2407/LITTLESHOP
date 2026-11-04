# Hướng dẫn LITTLE SHOP

Tài liệu được tách theo từng milestone để dễ setup và kiểm thử.

## Milestone 1 - Prototype mua và đặt máy

Hướng dẫn chuẩn bị project, tạo scene, cấu hình player, Computer UI, mua máy,
giao hàng, mở box và đặt máy:

[HUONG_DAN_MILESTONE_1.md](./HUONG_DAN_MILESTONE_1.md)

## Milestone 2 - Customer và vận hành cửa hàng

Hướng dẫn trạng thái máy, customer, NavMesh, quầy coin, PlayPoint, doanh thu,
cửa tự mở và animation:

[HUONG_DAN_MILESTONE_2.md](./HUONG_DAN_MILESTONE_2.md)

## Milestone 3 - Chu kỳ ngày và quản lý vận hành

Hướng dẫn mở/đóng cửa hàng, giới hạn thời gian trong ngày và các bước mở rộng
thống kê, bảo trì máy, tổng kết doanh thu:

[HUONG_DAN_MILESTONE_3.md](./HUONG_DAN_MILESTONE_3.md)

## Thứ tự thực hiện

1. Hoàn thành và kiểm thử Milestone 1 trước.
2. Đọc phần cấu hình NavMesh trong Milestone 2 trước khi tạo customer.
3. Sau đó cấu hình `CustomerManager`, `PlayPoint`, coin và Animator.
4. Cấu hình `ShopDayCycle` theo [Hướng dẫn Milestone 3](./HUONG_DAN_MILESTONE_3.md).
