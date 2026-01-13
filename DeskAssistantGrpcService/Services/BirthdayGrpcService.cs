using BirthdaysGrpcService;
using DeskAssistant.Core.Services;
using DeskAssistantGrpcService.Extensions;
using Grpc.Core;
using NLog;
using ILogger = NLog.ILogger;

namespace DeskAssistantGrpcService.Services
{
    public class BirthdayGrpcService : BirthdayService.BirthdayServiceBase
    {
        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();
        private readonly IBirthdaysService _birthdayService;
        private BirthdayItemExtensions _birthdayExtensions = new();


        public BirthdayGrpcService(IBirthdaysService birthdaysService)
        {
            _birthdayService = birthdaysService;
        }



        public override async Task<BirthdaySetResponse> SetBirthdays(BirthdayItem birthdayItem, ServerCallContext context)
        {
            try
            {
                var response = new BirthdaySetResponse();
                var birthdaEntity = _birthdayExtensions.GrpcBirthdayItemToBirthdaysEntity(birthdayItem);
                await _birthdayService.AddBirthdaysToDbAsync(birthdaEntity);

                response.Success = true;
                response.Message = "День рождения успешно отправлен в БД";

                return response;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "gRPC: Ошибка отправки дня рождения");

                return new BirthdaySetResponse
                {
                    Success = false,
                    Message = $"Ошибка: {ex.Message}"
                };
            }
        }

        public override async Task<BirthdayGetResponse> GetBirthdays(BirthdayEmptyRequest empty, ServerCallContext context)
        {
            try
            {
                var response = new BirthdayGetResponse();
                var allBirthdays = await _birthdayService.GetBirthdaysFromDbAsync();
                foreach (var birthday in allBirthdays)
                {
                    var birthdayItem = _birthdayExtensions.BirthdaysEntityToGrpcBirthdayItem(birthday);

                    response.Birthdays.Add(birthdayItem);
                }

                response.Success = true;
                response.Message = "Все дни рождения успешно получены";

                return response;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "gRPC: Ошибка получения задач");

                return new BirthdayGetResponse
                {
                    Success = false,
                    Message = $"Ошибка: {ex.Message}"
                };
            }
        }
    }
}
