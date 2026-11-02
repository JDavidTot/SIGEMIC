USE sigemic;

DELIMITER $$

CREATE PROCEDURE sp_estado_cuenta_cliente(
    IN p_id_cliente INT UNSIGNED
)
BEGIN

    -- Validar que el cliente exista
    IF NOT EXISTS (
        SELECT 1
        FROM clientes
        WHERE id_cliente = p_id_cliente
    ) THEN
        SIGNAL SQLSTATE '45000'
        SET MESSAGE_TEXT = 'El cliente indicado no existe';
    END IF;


    -- Generar estado de cuenta
    SELECT
        c.id_cliente AS 'ID Cliente',
        c.codigo_cliente AS 'Código cliente',

        CONCAT(
            u.nombres,
            ' ',
            u.apellidos
        ) AS 'Cliente',

        c.estado AS 'Estado cliente',

        (
            SELECT COUNT(*)
            FROM creditos cr
            WHERE cr.id_cliente = c.id_cliente
        ) AS 'Total créditos',

        COALESCE(
            (
                SELECT SUM(cr.monto_original)
                FROM creditos cr
                WHERE cr.id_cliente = c.id_cliente
            ),
            0
        ) AS 'Total otorgado',

        COALESCE(
            (
                SELECT SUM(cr.saldo_capital)
                FROM creditos cr
                WHERE cr.id_cliente = c.id_cliente
            ),
            0
        ) AS 'Saldo capital',

        COALESCE(
            (
                SELECT SUM(p.monto_total)
                FROM pagos p
                INNER JOIN creditos cr
                    ON cr.id_credito = p.id_credito
                WHERE cr.id_cliente = c.id_cliente
            ),
            0
        ) AS 'Total pagado',

        (
            SELECT COUNT(*)
            FROM cuotas cu
            INNER JOIN creditos cr
                ON cr.id_credito = cu.id_credito
            WHERE
                cr.id_cliente = c.id_cliente
                AND cu.estado = 'PAGADA'
        ) AS 'Cuotas pagadas',

        (
            SELECT COUNT(*)
            FROM cuotas cu
            INNER JOIN creditos cr
                ON cr.id_credito = cu.id_credito
            WHERE
                cr.id_cliente = c.id_cliente
                AND cu.estado IN (
                    'PENDIENTE',
                    'PARCIAL',
                    'ATRASADA'
                )
        ) AS 'Cuotas pendientes',

        (
            SELECT MIN(cu.fecha_vencimiento)
            FROM cuotas cu
            INNER JOIN creditos cr
                ON cr.id_credito = cu.id_credito
            WHERE
                cr.id_cliente = c.id_cliente
                AND cu.estado IN (
                    'PENDIENTE',
                    'PARCIAL',
                    'ATRASADA'
                )
        ) AS 'Próximo vencimiento'

    FROM clientes c

    INNER JOIN usuarios u
        ON u.id_usuario = c.id_usuario

    WHERE c.id_cliente = p_id_cliente;

END$$

DELIMITER ;