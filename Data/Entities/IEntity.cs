namespace Sistema.Data.Entities
{
    public interface IEntity
    {
        //comum em todas as entidades
        //todas as minhas entidades implementam isso, mesmo que não tenham métodos em comum".
        int Id { get; set; }  // toda entidade tem uma PK


    }
}
