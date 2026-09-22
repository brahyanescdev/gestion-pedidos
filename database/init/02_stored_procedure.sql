CREATE OR REPLACE FUNCTION get_customer_order_summary(p_customer_id UUID)
RETURNS TABLE (
    customer_id UUID,
    total_orders BIGINT,
    total_spent NUMERIC,
    last_order_date TIMESTAMPTZ
)
LANGUAGE plpgsql
AS $$
BEGIN
    RETURN QUERY
    SELECT
        o.customer_id,
        COUNT(DISTINCT o.id) AS total_orders,
        COALESCE(SUM(oi.quantity * oi.unit_price), 0)::NUMERIC AS total_spent,
        MAX(o.order_date) AS last_order_date
    FROM orders o
    LEFT JOIN order_items oi ON oi.order_id = o.id
    WHERE o.customer_id = p_customer_id
      AND o.status <> 'Cancelled'
    GROUP BY o.customer_id;
END;
$$;
