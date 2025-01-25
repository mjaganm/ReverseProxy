
#pragma once

#include <rocksdb/db.h>
#include <rocksdb/options.h>
#include <iostream>
#include <string>

class localstore {
    public:
    std::unique_ptr<rocksdb::DB> InitializeDB(const std::string& db_path, const rocksdb::Options& options);

    bool PutKeyValue(std::unique_ptr<rocksdb::DB>& db, const std::string& key, const std::string& value);

    bool GetValue(std::unique_ptr<rocksdb::DB>& db, const std::string& key, std::string& value);

    bool DeleteKey(std::unique_ptr<rocksdb::DB>& db, const std::string& key);

    int run();
};