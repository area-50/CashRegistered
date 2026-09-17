using Application.Financial.Interfaces;
using Domain.Financial.Entities;
using Domain.Financial.Interfaces;
using Domain.Shared.Notifications;
using Shared.Financial.Request;
using Shared.Financial.Response;

namespace Application.Financial.UseCases;

public class FinancialConfigurationUseCase(
    IFinancialConfigurationRepository repository,
    NotificationContext notificationContext,
    IUnitOfWork unitOfWork
) : IFinancialConfigurationUseCase
{
    public async Task<GetFinancialConfigurationResponse> GetFinancialConfiguration()
    {
        var config = await repository.GetActiveConfigurationAsync();

        if (config == null)
        {
            config = new FinancialConfiguration(
                approvalThresholdAmount: 0m,
                enableApprovalWorkflow: false,
                allowAutoApprovalForManagers: false,
                defaultInterestDailyRate: 0m,
                defaultFineRate: 0m
            );
            await repository.CreateAsync(config);
            await unitOfWork.CommitAsync();
        }

        return new GetFinancialConfigurationResponse
        {
            Id = config.Id,
            ApprovalThresholdAmount = config.ApprovalThresholdAmount,
            EnableApprovalWorkflow = config.EnableApprovalWorkflow,
            AllowAutoApprovalForManagers = config.AllowAutoApprovalForManagers,
            DefaultInterestDailyRate = config.DefaultInterestDailyRate,
            DefaultFineRate = config.DefaultFineRate,
            IsActive = config.IsActive
        };
    }

    public async Task<UpdateFinancialConfigurationResponse> UpdateFinancialConfiguration(UpdateFinancialConfigurationRequest request)
    {
        var config = await repository.GetActiveConfigurationAsync();

        if (config == null)
        {
            config = new FinancialConfiguration(
                approvalThresholdAmount: request.ApprovalThresholdAmount,
                enableApprovalWorkflow: request.EnableApprovalWorkflow,
                allowAutoApprovalForManagers: request.AllowAutoApprovalForManagers,
                defaultInterestDailyRate: request.DefaultInterestDailyRate,
                defaultFineRate: request.DefaultFineRate
            );

            if (config.IsInvalid)
            {
                notificationContext.AddNotifications(config.Notifications);
                return new UpdateFinancialConfigurationResponse { Id = 0 };
            }

            await repository.CreateAsync(config);
        }
        else
        {
            config.UpdateSettings(
                approvalThresholdAmount: request.ApprovalThresholdAmount,
                enableApprovalWorkflow: request.EnableApprovalWorkflow,
                allowAutoApprovalForManagers: request.AllowAutoApprovalForManagers,
                defaultInterestDailyRate: request.DefaultInterestDailyRate,
                defaultFineRate: request.DefaultFineRate
            );

            if (config.IsInvalid)
            {
                notificationContext.AddNotifications(config.Notifications);
                return new UpdateFinancialConfigurationResponse { Id = 0 };
            }

            repository.Update(config);
        }

        await unitOfWork.CommitAsync();

        return new UpdateFinancialConfigurationResponse { Id = config.Id };
    }
}
