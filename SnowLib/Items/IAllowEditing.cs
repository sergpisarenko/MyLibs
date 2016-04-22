using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnowLib.Items
{
    /// <summary>
    /// Интерфейс, разрешающий продолжение
    /// редактирования ранее считанного элемента 
    /// и обновляющий его значение на основе вновь 
    /// считанного из базы элемента
    /// </summary>
    /// <typeparam name="TItem">Тип элемента</typeparam>
    public interface IAllowEditing<TItem>
    {
        /// <summary>
        /// Сравнивает текущий, обновленный экземляр элемента 
        /// с исходным, сохраненным перед редактированием, 
        /// при необходимости, обновляет редактируемый
        /// </summary>
        /// <param name="operation">Операция редактирования</param>
        /// <returns>True, если редактирование может быть продолжено
        /// и False в противном случае<s/returns>
        bool ContinueEditing(IEditingOperation<TItem> operation);
    }
}
