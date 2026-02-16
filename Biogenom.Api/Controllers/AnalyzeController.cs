using Biogenom.Application.Commands;
using Biogenom.Application.DTOs.Requests;
using Biogenom.Application.DTOs.Results;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Biogenom.Api.Controllers
{
    /// <summary>
    /// Контроллер обеспечивает
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AnalyzeController: ControllerBase
    {
        private readonly IMediator _mediator;

        public AnalyzeController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("analyze")]
        [Consumes("application/json")]
        [SwaggerOperation(
            Summary = "Анализирует фото по ссылке.",
            Description = "Возвращает результат анализа GigaChat отправленной фотографии. 200, если обработка прошла успешна, 400 - если было отправлено не изображение"
            )]
        [SwaggerResponse(StatusCodes.Status200OK, "Изображение успешно проанализировано", typeof(AnalyzeImageResult))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Изображение по ссылке не обработано")]
        public async Task<ActionResult<AnalyzeImageResult>> Analyze([FromBody] AnalyzeItemsRequest request)
        {
            var command = new AnalyzeImageCommand { ImageUrl = request.ImageUrl};
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpPost("confirm")]
        [Consumes("application/json")]
        [SwaggerOperation(
            Summary = "Анализирует предметы по id запроса и переданым предметам.",
            Description = "Возвращает результат анализа GigaChat - предметы и материалы из которых они сделаны. 200, если обработка прошла успешна, 404 - если запрос не был найден"
            )]
        [SwaggerResponse(StatusCodes.Status200OK, "Прдметы успешно проанализировано", typeof(ConfirmItemsResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Такого запроса или предметов в запросе нет.")]
        public async Task<ActionResult<ConfirmItemsResult>> Confirm(int id, [FromBody] ConfirmItemsRequest request)
        {
            var commend = new ConfirmItemsCommand {RequestId = id, Items = request.Items };
            var result = await _mediator.Send(commend);
            return Ok(result);
        }
    }
}
