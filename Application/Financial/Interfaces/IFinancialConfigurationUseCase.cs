
namespace Application.Financial.Interfaces;

public interface IFinancialConfigurationUseCase
{
    Task<GetFinancialConfigurationResponse> GetFinancialConfiguration();
    Task<UpdateFinancialConfigurationResponse> UpdateFinancialConfiguration(UpdateFinancialConfigurationRequest request);
}
