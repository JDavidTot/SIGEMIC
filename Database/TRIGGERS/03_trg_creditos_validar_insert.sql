USE sigemic;

DELIMITER $$

CREATE TRIGGER trg_creditos_validar_insert
BEFORE INSERT ON creditos
FOR EACH ROW
BEGIN

    DECLARE v_estado_cliente VARCHAR(20);
    DECLARE v_estado_plan VARCHAR(20);

    -- =====================================================
    -- 1. VALIDAR MONTO DEL CRÉDITO
    -- =====================================================

    IF NEW.monto_original <= 0 THEN

        SIGNAL SQLSTATE '45000'
        SET MESSAGE_TEXT =
            'El monto del crédito debe ser mayor que cero';

    END IF;


    -- =====================================================
    -- 2. VALIDAR CUOTA
    -- =====================================================

    IF NEW.cuota_original <= 0 THEN

        SIGNAL SQLSTATE '45000'
        SET MESSAGE_TEXT =
            'La cuota del crédito debe ser mayor que cero';

    END IF;


    -- =====================================================
    -- 3. VALIDAR SALDO DE CAPITAL
    -- =====================================================

    IF NEW.saldo_capital < 0
       OR NEW.saldo_capital > NEW.monto_original THEN

        SIGNAL SQLSTATE '45000'
        SET MESSAGE_TEXT =
            'El saldo de capital no es válido';

    END IF;


    -- =====================================================
    -- 4. VALIDAR PLAZO PERMITIDO
    -- =====================================================

    IF NEW.plazo_meses NOT IN
       (6, 12, 18, 24, 36, 48, 60) THEN

        SIGNAL SQLSTATE '45000'
        SET MESSAGE_TEXT =
            'El plazo del crédito no está permitido';

    END IF;


    -- =====================================================
    -- 5. VALIDAR PORCENTAJE DE INTERÉS
    -- =====================================================

    IF NEW.porcentaje_interes_aplicado <= 0 THEN

        SIGNAL SQLSTATE '45000'
        SET MESSAGE_TEXT =
            'El porcentaje de interés debe ser mayor que cero';

    END IF;


    -- =====================================================
    -- 6. VALIDAR FECHAS
    -- =====================================================

    IF NEW.fecha_inicio < NEW.fecha_otorgamiento THEN

        SIGNAL SQLSTATE '45000'
        SET MESSAGE_TEXT =
            'La fecha de inicio no puede ser anterior al otorgamiento';

    END IF;


    IF NEW.fecha_finalizacion_estimada <= NEW.fecha_inicio THEN

        SIGNAL SQLSTATE '45000'
        SET MESSAGE_TEXT =
            'La fecha de finalización debe ser posterior a la fecha de inicio';

    END IF;


    -- =====================================================
    -- 7. VALIDAR CLIENTE ACTIVO
    -- =====================================================

    SET v_estado_cliente =
    (
        SELECT estado
        FROM clientes
        WHERE id_cliente = NEW.id_cliente
        LIMIT 1
    );

    IF v_estado_cliente IS NULL THEN

        SIGNAL SQLSTATE '45000'
        SET MESSAGE_TEXT =
            'El cliente indicado no existe';

    END IF;


    IF v_estado_cliente <> 'ACTIVO' THEN

        SIGNAL SQLSTATE '45000'
        SET MESSAGE_TEXT =
            'El cliente debe estar activo para recibir un crédito';

    END IF;


    -- =====================================================
    -- 8. VALIDAR PLAN ACTIVO
    -- =====================================================

    SET v_estado_plan =
    (
        SELECT estado
        FROM planes_credito
        WHERE id_plan = NEW.id_plan
        LIMIT 1
    );

    IF v_estado_plan IS NULL THEN

        SIGNAL SQLSTATE '45000'
        SET MESSAGE_TEXT =
            'El plan de crédito indicado no existe';

    END IF;


    IF v_estado_plan <> 'ACTIVO' THEN

        SIGNAL SQLSTATE '45000'
        SET MESSAGE_TEXT =
            'El plan de crédito debe estar activo';

    END IF;

END$$

DELIMITER ;