using Biogenom.Application.DTOs.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Biogenom.Application.Commands
{
    /// <summary>
    /// Класс команды для подтверждения предметов, предсказанных на изображении. Содержит идентификатор запроса и список имен предметов, которые пользователь подтвердил. Реализует интерфейс <see cref="IRequest{ConfirmItemsResult}"/>, что позволяет использовать его с MediatR для обработки запроса и получения результата в виде <see cref="ConfirmItemsResult"/>. Этот класс служит для передачи информации о том, какие предметы были подтверждены пользователем после анализа изображения.
    /// </summary>
    public class ConfirmItemsCommand : IRequest<ConfirmItemsResult>
    {
        public int RequestId { get; set; }
        public List<string> Items { get; set; } = [];
    }
}
