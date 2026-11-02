USE sigemic;

DELIMITER $$

CREATE TRIGGER trg_pagos_validar_insert
BEFORE INSERT ON pagos
FOR EACH ROW
BEGIN

    DECLARE v_forma_pago VARCHAR(50);

    -- =====================================================
    -- 1. VALIDAR MONTO TOTAL
    -- =====================================================

    IF NEW.monto_total <= 0 THEN

        SIGNAL SQLSTATE '45000'
        SET MESSAGE_TEXT =
            'El monto del pago debe ser mayor que cero';

    END IF;


    -- =====================================================
    -- 2. VALIDAR VALORES NEGATIVOS
    -- =====================================================

    IF NEW.capital_aplicado < 0
       OR NEW.interes_aplicado < 0
       OR NEW.mora_aplicada < 0 THEN

        SIGNAL SQLSTATE '45000'
        SET MESSAGE_TEXT =
            'Capital, interés y mora no pueden ser negativos';

    END IF;


    -- =====================================================
    -- 3. VALIDAR DISTRIBUCIÓN DEL PAGO
    -- capital + interés + mora = monto total
    -- =====================================================

    IF ROUND(
        NEW.capital_aplicado +
        NEW.interes_aplicado +
        NEW.mora_aplicada,
        2
    ) <> ROUND(NEW.monto_total, 2) THEN

        SIGNAL SQLSTATE '45000'
        SET MESSAGE_TEXT =
            'La distribución del pago no coincide con el monto total';

    END IF;


    -- =====================================================
    -- 4. PAGO DE CUOTA DEBE TENER CUOTA
    -- =====================================================

    IF NEW.tipo_operacion = 'PAGO_CUOTA'
       AND NEW.id_cuota IS NULL THEN

        SIGNAL SQLSTATE '45000'
        SET MESSAGE_TEXT =
            'Un pago de cuota debe tener una cuota asociada';

    END IF;


    -- =====================================================
    -- 5. ABONO EXTRAORDINARIO NO DEBE ESTAR
    --    ASOCIADO A UNA CUOTA ESPECÍFICA
    -- =====================================================

    IF NEW.tipo_operacion = 'ABONO_EXTRAORDINARIO'
       AND NEW.id_cuota IS NOT NULL THEN

        SIGNAL SQLSTATE '45000'
        SET MESSAGE_TEXT =
            'Un abono extraordinario no debe tener una cuota asociada';

    END IF;


    -- =====================================================
    -- 6. VALIDAR FORMA DE PAGO ACTIVA
    -- =====================================================

    SET v_forma_pago =
    (
        SELECT nombre
        FROM formas_pago
        WHERE
            id_forma_pago = NEW.id_forma_pago
            AND estado = 'ACTIVO'
        LIMIT 1
    );


    IF v_forma_pago IS NULL THEN

        SIGNAL SQLSTATE '45000'
        SET MESSAGE_TEXT =
            'La forma de pago indicada no existe o está inactiva';

    END IF;


    -- =====================================================
    -- 7. SI ES EFECTIVO NO DEBE TENER DATOS BANCARIOS
    -- =====================================================

    IF v_forma_pago = 'EFECTIVO'
       AND (
            NEW.id_banco IS NOT NULL
            OR NEW.numero_boleta IS NOT NULL
       ) THEN

        SIGNAL SQLSTATE '45000'
        SET MESSAGE_TEXT =
            'Un pago en efectivo no debe contener información bancaria';

    END IF;


    -- =====================================================
    -- 8. TRANSFERENCIA O DEPÓSITO REQUIEREN BANCO Y NÚMERO DE BOLETA
    -- =====================================================

    IF v_forma_pago IN ('TRANSFERENCIA', 'DEPOSITO')
       AND (
            NEW.id_banco IS NULL
            OR NEW.numero_boleta IS NULL
            OR TRIM(NEW.numero_boleta) = ''
       ) THEN

        SIGNAL SQLSTATE '45000'
        SET MESSAGE_TEXT =
            'Los pagos bancarios requieren banco y número de boleta';

    END IF;

END$$

DELIMITER ;