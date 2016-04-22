namespace SnowLib.DB
{
    /// <summary>
    /// Интерфейс, сигнализируюшщий о начале и окончании 
    /// загрузки свойств из различных источников
    /// </summary>
    public interface ISpmUpdateProperties
    {
        void BeginUpdateProperties();
        void EndUpdateProperties();
    }
}
