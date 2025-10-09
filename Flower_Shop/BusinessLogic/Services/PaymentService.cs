using AutoMapper;
using BusinessLogic.DTOs.Payments;
using BusinessLogic.Services.Interfaces;
using DataAccess.Entities;
using DataAccess.UnitOfWork;
using Ultitity.Exceptions;

namespace BusinessLogic.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public PaymentService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<PaymentDto?> GetPaymentByOrderIdAsync(Guid orderId)
        {
            var payment = await _unitOfWork.Payment.GetAsync(p => p.OrderId == orderId);
            return payment == null ? null : _mapper.Map<PaymentDto>(payment);
        }

        public async Task<PaymentDto> UpdatePaymentStatusAsync(PaymentUpdateStatusRequest request)
        {
            var paymentToUpdate = await _unitOfWork.Payment.GetAsync(p =>
                p.PaymentId == request.PaymentId
            );
            if (paymentToUpdate == null)
            {
                throw new KeyNotFoundException($"Payment with ID {request.PaymentId} not found.");
            }

            if (Enum.TryParse<PaymentStatus>(request.PaymentStatus, true, out var newStatus))
            {
                paymentToUpdate.Status = newStatus;

                // Nếu thanh toán thành công, ghi nhận thời gian
                if (newStatus == PaymentStatus.Accepted)
                {
                    paymentToUpdate.PaidAt = DateTime.UtcNow;
                }
            }
            else
            {
                throw new CustomValidationException(
                    new Dictionary<string, string[]>
                    {
                        { "Status", new[] { $"Invalid payment status: {request.PaymentStatus}." } },
                    }
                );
            }

            if (!string.IsNullOrEmpty(request.TransactionId))
            {
                paymentToUpdate.TransactionId = request.TransactionId;
            }

            await _unitOfWork.SaveAsync();
            return _mapper.Map<PaymentDto>(paymentToUpdate);
        }
    }
}
