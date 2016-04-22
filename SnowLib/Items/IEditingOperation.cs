using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnowLib.Items
{
    /// <summary>
    /// Интерфейс описания элемента, находящегося
    /// в процессе редактирования
    /// </summary>
    /// <typeparam name="TItem">Тип элемента</typeparam>
    public interface IEditingOperation<TItem>
    {
        /// <summary>
        /// Элемент, в который сохраняются значения
        /// редактируемого элемента
        /// </summary>
        TItem Backup { get; }

        /// <summary>
        /// Текущий редактируемый элемент
        /// </summary>
        TItem Edited { get; }
        
        /// <summary>
        /// В процессе редактирование оповещает
        /// об изменениях, и, возможно, прекращает
        /// редактирование
        /// </summary>
        /// <param name="info">Пояснение об изменениях</param>
        /// <param name="allowApplyChanges">Если False, то запрещает сохранение
        /// текущих изменений</param>
        void AlertEditing(string info, bool allowApplyChanges = false);
    }
}
