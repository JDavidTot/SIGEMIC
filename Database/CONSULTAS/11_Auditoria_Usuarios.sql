USE sigemic;

SELECT
    COALESCE(u.username, 'SIN USUARIO') AS 'Usuario',
    a.accion AS 'Acción',
    a.resultado AS 'Resultado',
    COUNT(*) AS 'Cantidad',
    MIN(a.fecha_hora) AS 'Primer registro',
    MAX(a.fecha_hora) AS 'Último registro'
FROM auditoria a
LEFT JOIN usuarios u
    ON u.id_usuario = a.id_usuario
GROUP BY
    u.username,
    a.accion,
    a.resultado
ORDER BY
    COUNT(*) DESC,
    MAX(a.fecha_hora) DESC;