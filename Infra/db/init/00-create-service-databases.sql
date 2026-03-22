SELECT 'CREATE DATABASE userdb'
WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'userdb')\gexec