INSERT INTO customers (id, name, email) VALUES
    ('11111111-1111-1111-1111-111111111111', 'Ana Torres', 'ana.torres@example.com'),
    ('22222222-2222-2222-2222-222222222222', 'Luis Gómez', 'luis.gomez@example.com')
ON CONFLICT (id) DO NOTHING;

INSERT INTO products (id, name, price, stock) VALUES
    ('33333333-3333-3333-3333-333333333333', 'Teclado mecánico', 45.90, 50),
    ('44444444-4444-4444-4444-444444444444', 'Mouse inalámbrico', 19.50, 100),
    ('55555555-5555-5555-5555-555555555555', 'Monitor 24 pulgadas', 139.99, 25)
ON CONFLICT (id) DO NOTHING;
