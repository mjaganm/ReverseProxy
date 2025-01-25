#include <rocksdb/db.h>
#include <rocksdb/options.h>
#include <iostream>
#include <string>

// Function to initialize the RocksDB instance
rocksdb::DB* InitializeDB(const std::string& db_path, const rocksdb::Options& options) {
    rocksdb::DB* db;
    rocksdb::Status status = rocksdb::DB::Open(options, db_path, &db);
    if (!status.ok()) {
        std::cerr << "Error opening database: " << status.ToString() << std::endl;
        return nullptr;
    }
    return db;
}

// Function to put a key-value pair into the database
bool PutKeyValue(rocksdb::DB* db, const std::string& key, const std::string& value) {
    rocksdb::Status status = db->Put(rocksdb::WriteOptions(), key, value);
    if (!status.ok()) {
        std::cerr << "Error putting key-value: " << status.ToString() << std::endl;
        return false;
    }
    return true;
}

// Function to get a value by key from the database
bool GetValue(rocksdb::DB* db, const std::string& key, std::string& value) {
    rocksdb::Status status = db->Get(rocksdb::ReadOptions(), key, &value);
    if (!status.ok()) {
        if (status.IsNotFound()) {
            std::cerr << "Key not found: " << key << std::endl;
        } else {
            std::cerr << "Error getting value: " << status.ToString() << std::endl;
        }
        return false;
    }
    return true;
}

// Function to delete a key-value pair from the database
bool DeleteKey(rocksdb::DB* db, const std::string& key) {
    rocksdb::Status status = db->Delete(rocksdb::WriteOptions(), key);
    if (!status.ok()) {
        std::cerr << "Error deleting key: " << status.ToString() << std::endl;
        return false;
    }
    return true;
}

int main() {
    const std::string kDBPath = "./testdb";

    // Configure RocksDB options
    rocksdb::Options options;
    options.create_if_missing = true;

    // Initialize database
    rocksdb::DB* db = InitializeDB(kDBPath, options);
    if (!db) return -1;

    // Example usage
    if (PutKeyValue(db, "key1", "value1")) {
        std::cout << "Successfully inserted key1:value1" << std::endl;
    }

    if (PutKeyValue(db, "jagan", "mohan")) {
        std::cout << "Successfully inserted jagan:mohan" << std::endl;
    }

    for (long i = 0; i < 999999999; i++) {
        if (PutKeyValue(db, "key" + std::to_string(i), "value1+" + std::to_string(i))) {
            if (i % 100 == 0) {
                std::cout << "Successfully inserted key+" << i << ":value1" << i << std::endl;
            }
        }
    }

    std::string value;
    if (GetValue(db, "key1", value)) {
        std::cout << "Retrieved key1: " << value << std::endl;
    }

    if (DeleteKey(db, "key1")) {
        std::cout << "Successfully deleted key1" << std::endl;
    }

    // jagan
    if (GetValue(db, "jagan", value)) {
        std::cout << "Retrieved key: jagan: " << value << std::endl;
    }

    if (DeleteKey(db, "key1")) {
        std::cout << "Successfully deleted key: jagan" << std::endl;
    }

    delete db;
    return 0;
}

