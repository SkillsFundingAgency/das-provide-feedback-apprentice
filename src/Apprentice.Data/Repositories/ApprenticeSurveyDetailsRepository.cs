using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using ESFA.DAS.ProvideFeedback.Apprentice.Data.Dto;
using Polly;

namespace ESFA.DAS.ProvideFeedback.Apprentice.Data.Repositories
{
    public class ApprenticeSurveyInvitesRepository : IStoreApprenticeSurveyDetails
    {
        private const int _commandTimeoutSeconds = 120;
        private readonly IDbConnection _dbConnection;

        public ApprenticeSurveyInvitesRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public Task<IEnumerable<ApprenticeSurveyInvite>> GetApprenticeSurveyInvitesAsync(int batchSize)
        {
            return _dbConnection.QueryAsync<ApprenticeSurveyInvite>(sql: $@"
                                        SELECT TOP {batchSize} * 
                                        FROM ApprenticeSurveyInvites
                                        WHERE SentDate IS NULL", param: null, transaction: null, commandTimeout: _commandTimeoutSeconds);
        }

        public Task SetApprenticeSurveySentAsync(Guid apprenticeSurveyId)
        {
            var now = DateTime.Now;
            var expiry = now.AddDays(7);
            return SetApprenticeSurveyDates(apprenticeSurveyId, now, expiry);
        }

        public Task SetApprenticeSurveyNotSentAsync(Guid apprenticeSurveyId)
        {
            return SetApprenticeSurveyDates(apprenticeSurveyId, null, null);
        }

        private Task SetApprenticeSurveyDates(Guid apprenticeSurveyId, DateTime? sentDate, DateTime? expiryDate)
        {
            var sql = $@"
                        UPDATE ApprenticeSurveyInvites
                        SET SentDate = @{nameof(sentDate)}, ExpiryDate = @{nameof(expiryDate)}
                        WHERE Id = @{nameof(apprenticeSurveyId)}";

            return ExecuteUpdateAsync(sql, new { sentDate, expiryDate, apprenticeSurveyId });
        }

        private Task ExecuteUpdateAsync(string sql, object param)
        {
            var policy = Policy.Handle<Exception>()
                .WaitAndRetryAsync(3, x => TimeSpan.FromSeconds(3));

            return policy.ExecuteAsync(() =>
             {
                 return _dbConnection.QueryAsync(sql: sql, param: param, transaction: null, commandTimeout: _commandTimeoutSeconds);
             });
        }
    }
}
