using System.Linq;
using System.Text;
using DataAccess.Entities;

namespace BusinessLogic.Services
{
    public static class EmailTemplateService
    {
        #region Base Template
        private static string BuildBaseEmail(string title, string contentHtml)
        {
            return $@"
            <!DOCTYPE html>
            <html lang=""vi"">
            <head>
                <meta charset=""UTF-8"">
                <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                <style>
                    body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; margin: 0; padding: 0; background-color: #f0f8ff; }}
                    .container {{ width: 100%; max-width: 600px; margin: 20px auto; background-color: #ffffff; border: 1px solid #ffc0cb; border-radius: 12px; overflow: hidden; box-shadow: 0 4px 15px rgba(0,0,0,0.05); }}
                    .header {{ background: linear-gradient(135deg, #ffc0cb, #b0e0e6); padding: 20px; text-align: center; }}
                    .header h1 {{ color: white; margin: 0; font-size: 24px; }}
                    .content {{ padding: 30px; color: #333; line-height: 1.6; }}
                    .footer {{ background-color: #f8f9fa; padding: 20px; text-align: center; font-size: 12px; color: #777; }}
                    .order-table {{ width: 100%; border-collapse: collapse; margin-top: 20px; }}
                    .order-table th, .order-table td {{ border: 1px solid #eee; padding: 12px; }}
                    .order-table th {{ background-color: #fdf2f4; color: #d15a7c; }}
                    .total-row {{ font-weight: bold; }}
                </style>
            </head>
            <body>
                <div class=""container"">
                    <div class=""header"">
                        <h1>FlowerShop</h1>
                    </div>
                    <div class=""content"">
                        <h2>{title}</h2>
                        {contentHtml}
                    </div>
                    <div class=""footer"">
                        <p>&copy; {DateTime.Now.Year} FlowerShop. All rights reserved.</p>
                        <p>Đây là email tự động, vui lòng không trả lời.</p>
                    </div>
                </div>
            </body>
            </html>";
        }
        #endregion

        #region Email Templates

        public static string PaymentSuccessEmail(Order order, User user)
        {
            var title = $"Thanh toán thành công đơn hàng #{order.OrderNumber}";
            var content =
                $@"
                <p>Xin chào <strong>{user.FullName}</strong>,</p>
                <p>Cảm ơn bạn! FlowerShop đã xác nhận thanh toán thành công cho đơn hàng của bạn. Chúng tôi đang tiến hành chuẩn bị đơn hàng và sẽ giao đến bạn trong thời gian sớm nhất.</p>
                {BuildOrderDetailsHtml(order)}
                <p>Trân trọng,<br>Đội ngũ FlowerShop</p>";

            return BuildBaseEmail(title, content);
        }

        public static string OrderConfirmedEmail(Order order, User user)
        {
            var title = $"Đơn hàng #{order.OrderNumber} đã được xác nhận";
            var content =
                $@"
                <p>Xin chào <strong>{user.FullName}</strong>,</p>
                <p>Đơn hàng của bạn đã được xác nhận và đang trong quá trình chuẩn bị. Vui lòng chuẩn bị số tiền <strong>{order.TotalAmount:N0} VND</strong> để thanh toán khi nhận hàng.</p>
                {BuildOrderDetailsHtml(order)}
                <p>Trân trọng,<br>Đội ngũ FlowerShop</p>";
            return BuildBaseEmail(title, content);
        }

        private static string BuildOrderDetailsHtml(Order order)
        {
            var itemsHtml = new StringBuilder();
            foreach (var item in order.Items)
            {
                var productName = item.Product?.Name ?? "Sản phẩm không xác định";
                itemsHtml.Append(
                    $@"
                    <tr>
                        <td style='padding: 12px; border: 1px solid #eee;'>{productName}</td>
                        <td style='padding: 12px; border: 1px solid #eee; text-align: center;'>{item.Quantity}</td>
                        <td style='padding: 12px; border: 1px solid #eee; text-align: right;'>{item.UnitPrice:N0} VND</td>
                        <td style='padding: 12px; border: 1px solid #eee; text-align: right;'>{item.LineTotal:N0} VND</td>
                    </tr>"
                );
            }

            return $@"
                <h3 style='border-bottom: 2px solid #ffc0cb; padding-bottom: 5px; margin-top: 25px;'>Chi tiết đơn hàng</h3>
                <p><strong>Mã đơn hàng:</strong> #{order.OrderNumber}</p>
                <p><strong>Ngày đặt:</strong> {order.CreatedAt:dd/MM/yyyy}</p>
                <p><strong>Địa chỉ giao hàng:</strong> {order.ShippingAddress}</p>
                
                <table class='order-table'>
                    <thead>
                        <tr>
                            <th>Sản phẩm</th>
                            <th>Số lượng</th>
                            <th>Đơn giá</th>
                            <th>Thành tiền</th>
                        </tr>
                    </thead>
                    <tbody>
                        {itemsHtml}
                        <tr class='total-row'>
                            <td colspan='3' style='padding: 12px; border: 1px solid #eee; text-align: right;'>Tổng cộng</td>
                            <td style='padding: 12px; border: 1px solid #eee; text-align: right;'>{order.TotalAmount:N0} VND</td>
                        </tr>
                    </tbody>
                </table>";
        }

        #endregion
    }
}
