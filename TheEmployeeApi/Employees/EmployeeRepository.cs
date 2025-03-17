using TheEmployeeApi.Employees;


namespace TheEmployeeApi.Employees
{
    public class EmployeeRepository: IRepository<Employee>
    {
        private readonly List<Employee> _employees = new ();

        public Employee? GetById(int id)
        {
            return _employees.SingleOrDefault(e => e.Id == id);
        }

        public IEnumerable<Employee> GetAll()
        {
            return _employees;
        }

        public void Create(Employee entity)
        {
            int LatestEmployeeId = 0;
            if(_employees.Count > 0)
            {
                LatestEmployeeId = _employees.Last().Id;
            }
        
            int NewEmployeeId = LatestEmployeeId + 1;
            var NewEmployee = new Employee { Id = NewEmployeeId, FirstName = entity.FirstName, LastName = entity.LastName, SocialSecurityNumber = entity.SocialSecurityNumber };
            _employees.Add(NewEmployee);
        }

        public void Update(Employee entity)
        {
            if(entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }
            var existingEmployee = GetById(entity.Id);
            if(existingEmployee == null)
            {
                throw new InvalidOperationException("Employee not found");
            }

            if (existingEmployee != null)
            {
                existingEmployee.FirstName = entity.FirstName;
                existingEmployee.LastName = entity.LastName;
            }

        }

        public void Delete(Employee entity)
        {
            if(entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }
            _employees.Remove(entity);
        }

    }
}
