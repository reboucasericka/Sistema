-- Verificar se o admin existe
SELECT Id, Email, UserName, EmailConfirmed, Active FROM AspNetUsers WHERE Email = 'admin@admin.com';

-- Verificar roles do admin
SELECT u.Email, r.Name as RoleName 
FROM AspNetUsers u
JOIN AspNetUserRoles ur ON u.Id = ur.UserId
JOIN AspNetRoles r ON ur.RoleId = r.Id
WHERE u.Email = 'admin@admin.com';
