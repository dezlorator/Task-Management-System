using API.DTO;
using API.Requests;
using BusinessLogic.Interfaces.Services;
using Domain.DTO;
using Domain.Enums;
using Infrastructure.MessageBroker;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TaskController : ControllerBase
    {
        private readonly ITaskService _taskService;

        public TaskController(ITaskService taskService)
        {
            _taskService = taskService;
        }

        [HttpGet]
        public async Task<IActionResult> GetById([FromQuery]GetByIdRequest getByIdRequest)
        {
            var result = await _taskService.GetByIdAsync(getByIdRequest.Id);

            return Ok(result);
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchTask([FromQuery]SearchTaskRequest seachTaskRequest)
        {
            var dto = new SearchTaskDTO(seachTaskRequest.PageSize, 
                                        seachTaskRequest.PageNumber,
                                        seachTaskRequest.Name, 
                                        seachTaskRequest.Description,
                                        seachTaskRequest.Status.HasValue ? (TaskModelStatus)seachTaskRequest.Status.Value : null,
                                        seachTaskRequest.AssignedTo,
                                        seachTaskRequest.SearchQuery);
            var result = await _taskService.Search(dto);

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateTaskRequest createTaskRequest)
        {
            var dto = new CreateTaskDTO(createTaskRequest.Name, 
                                        createTaskRequest.Description, 
                                        (TaskModelStatus)createTaskRequest.Status, 
                                        createTaskRequest.AssignedTo);
            var result = await _taskService.CreateTaskAsync(dto);

            return Ok(result);
        }

        [HttpPut]
        public async Task<IActionResult> Update(UpdateTaskRequest updateTaskRequest)
        {
            var dto = new UpdateTaskDTO(updateTaskRequest.Id, 
                                        updateTaskRequest.Name,
                                        updateTaskRequest.Description,
                                        updateTaskRequest.Status.HasValue ? (TaskModelStatus)updateTaskRequest.Status.Value : null);
            await _taskService.UpdateTaskAsync(dto);

            return Ok();
        }
    }
}
