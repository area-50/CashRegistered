using System.Linq.Expressions;
using Domain.Identity.Entities;
using Domain.Financial.Entities;
using Domain.Shared.Abstractions;

namespace Domain.Financial.Repositories;

public interface ICashFlowRepository : IRepository<CashFlow>
{
    
}