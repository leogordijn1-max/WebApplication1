SELECT
    u.Email,
    r.Name AS Rol
FROM AspNetUsers u
LEFT JOIN AspNetUserRoles ur ON u.Id = ur.UserId
LEFT JOIN AspNetRoles r ON ur.RoleId = r.Id;

SELECT Id, Name
FROM AspNetRoles;

SELECT u.Email, r.Name AS Rol
FROM AspNetUsers u
JOIN AspNetUserRoles ur ON u.Id = ur.UserId
JOIN AspNetRoles r ON ur.RoleId = r.Id
ORDER BY u.Email, r.Name;