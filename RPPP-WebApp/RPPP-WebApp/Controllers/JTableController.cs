using Microsoft.AspNetCore.Mvc;
using RPPP_WebApp.Model;

namespace RPPP_WebApp.Controllers
{

    /// <summary>
    /// Abstract controller class for handling generic JTable operations.
    /// This class provides endpoints for CRUD operations, working with a specific model and key type.
    /// </summary>
    /// <typeparam name="TController">The type of the controller that actually performs the operations.</typeparam>
    /// <typeparam name="TKey">The type of the key for the model.</typeparam>
    /// <typeparam name="TModel">The type of the model this controller works with.</typeparam>
    [ApiExplorerSettings(IgnoreApi = true)]
    [TypeFilter(typeof(ErrorStatusTo200WithErrorMessage))]
    public abstract class JTableController<TController, TKey, TModel> : ControllerBase where TController : ICustomController<TKey, TModel>
    {
        private readonly TController controller;

        public JTableController(TController controller)
        {
            this.controller = controller;
        }

        /// <summary>
        /// Retrieves a list of all records along with the total count. Supports optional filtering and pagination.
        /// </summary>
        /// <param name="loadParams">Parameters for loading data such as sorting, paging, etc.</param>
        /// <param name="search">Optional search term for filtering.</param>
        /// <returns>A collection of table records and the total count.</returns>
        [HttpPost]
        public virtual async Task<TableRecords<TModel>> GetAll([FromQuery] LoadParams loadParams, [FromForm] string search)
        {
            int count = await controller.Count(search);
            loadParams.Filter = search;
            var list = await controller.GetAll(loadParams);
            return new TableRecords<TModel>(count, list);
        }

        /// <summary>
        /// Creates a new record based on the provided model.
        /// </summary>
        /// <param name="model">The model to create a new record from.</param>
        /// <returns>A JTableAjaxResult indicating success or failure.</returns>
        [HttpPost]
        public virtual async Task<JTableAjaxResult> Create([FromForm] TModel model)
        {
            if (model == null)
            {
                return JTableAjaxResult.Error("Model is null");
            }
            else if (!ModelState.IsValid)
            {
                return JTableAjaxResult.Error(ModelState.GetErrorsString());
            }

            var result = await controller.Create(model);
            if (result is CreatedAtActionResult created)
            {
                return new CreateResult(created.Value);
            }
            else
            {
                return JTableAjaxResult.Error(result.ToString());
            }
        }

        /// <summary>
        /// Updates an existing record identified by the provided id with the data in the provided model.
        /// </summary>
        /// <param name="id">The id of the record to update.</param>
        /// <param name="model">The model containing updated data.</param>
        /// <returns>A JTableAjaxResult indicating success or failure.</returns>
        protected async Task<JTableAjaxResult> UpdateItem(TKey id, TModel model)
        {

            if (model == null)
            {
                return JTableAjaxResult.Error("Model is null");
            }
            else if (!ModelState.IsValid)
            {
                return JTableAjaxResult.Error(ModelState.GetErrorsString());
            }


            var result = await controller.Update(id, model);
            if (result is NoContentResult)
            {
                return JTableAjaxResult.OK;
            }
            else
            {
                return JTableAjaxResult.Error("Not found");
            }
        }

        /// <summary>
        /// Deletes a record identified by the provided id.
        /// </summary>
        /// <param name="id">The id of the record to delete.</param>
        /// <returns>A JTableAjaxResult indicating success or failure.</returns>
        protected async Task<JTableAjaxResult> DeleteItem(TKey id)
        {
            var result = await controller.Delete(id);
            if (result is NoContentResult)
            {
                return JTableAjaxResult.OK;
            }
            else
            {
                return JTableAjaxResult.Error("Not found");
            }
        }
    }
}
