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

        public async Task<IEnumerable<ApprenticeSurveyInvite>> GetApprenticeSurveyInvitesAsync(int batchSize)
        {
            return await _dbConnection.QueryAsync<ApprenticeSurveyInvite>(sql: $@"
                                        SELECT TOP {batchSize} * 
                                        FROM ApprenticeSurveyInvites
                                        WHERE SentDate IS NULL", param: null, transaction: null, commandTimeout: _commandTimeoutSeconds);
        }

        public async Task SetApprenticeSurveySentAsync(string uniqueLearnerNumber, string surveyCode)
        {
            var now = DateTime.Now;
            var expiry = now.AddDays(7);
            var sql = $@"
                        UPDATE ApprenticeSurveyInvites
                        SET SentDate = @{nameof(now)}, ExpiryDate = @{nameof(expiry)}
                        WHERE UniqueLearnerNumber = @{nameof(uniqueLearnerNumber)}
                        AND SurveyCode = @{nameof(surveyCode)}";

            await ExecuteUpdateAsync(sql, new { now, expiry, uniqueLearnerNumber, surveyCode });
        }

        private async Task ExecuteUpdateAsync(string sql, object param)
        {
            var policy = Policy.Handle<Exception>()
                .WaitAndRetryAsync(3, x => TimeSpan.FromSeconds(3));

            await policy.ExecuteAsync(() =>
            {
                return _dbConnection.QueryAsync(sql: sql, param: param, transaction: null, commandTimeout: _commandTimeoutSeconds);
            });
        }
    }
}
