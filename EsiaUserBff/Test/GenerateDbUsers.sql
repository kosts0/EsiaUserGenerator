BEGIN;

WITH new_requesthistory AS (
    INSERT INTO "RequestHistory"
        ("RequestId",
         "JsonRequest",
         "CurrentStatus",
         "DateTimeCreated",
         "Finished",
         "LastModified",
         "GeneratedUserInfo")
        SELECT
            gen_random_uuid(),
            jsonb_build_object(
                    'testField', floor(random() * 10000)
            ),
            'TestStatus',
            now(),
            true,
            now(),
            jsonb_build_object(
                    'id', gen_random_uuid(),
                    'status', 'active',
                    'document', jsonb_build_object(
                            'series', '1111',
                            'number', '666666',
                            'address', jsonb_build_object(
                                    'city', 'РФ'
                                       )
                                )

            )
        FROM generate_series(1, 10)
        RETURNING "RequestId"
)

INSERT INTO "EsiaUsers"
("Id", "Oid", "Login", "Password", "CreatedRequestId")
SELECT
    gen_random_uuid(),
    floor(random() * 1000)::int,
    '70000000000',
    'secretaryship',
    "RequestId"
FROM new_requesthistory;

COMMIT;
