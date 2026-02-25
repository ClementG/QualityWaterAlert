using QualityWaterAlert.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QualityWaterAlert.Infrastructure.Interfaces
{
    /// <summary>
    /// Defines the contract for a data provider that fetches water quality information.
    /// </summary>
    public interface IDataProvider
    {
        /// <summary>
        /// Asynchronously retrieves a comprehensive water quality analysis for a specific commune.
        /// </summary>
        /// <param name="inseeCode">The INSEE code of the commune to analyze.</param>
        /// <param name="startDate">The start date of the analysis period. If null, historical data is considered from the beginning.</param>
        /// <param name="endDate">The end date of the analysis period. If null, data is considered up to the most recent records.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="WaterQualityAnalysis"/> for the specified commune and date range.</returns>
        Task<WaterQualityAnalysis> GetWaterQualityAnalysisAsync(string inseeCode, DateTime? startDate, DateTime? endDate);

        /// <summary>
        /// Asynchronously retrieves all available communes.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of all <see cref="Commune"/> objects.</returns>
        Task<List<Commune>> GetAllCommunesAsync();
    }
}
