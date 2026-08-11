<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;

return new class extends Migration
{
    public function up(): void
    {
        Schema::table('professionals', function (Blueprint $table) {
            if (! Schema::hasColumn('professionals', 'biography')) {
                $table->text('biography')->nullable()->after('photo');
            }
            if (! Schema::hasColumn('professionals', 'instagram')) {
                $table->string('instagram')->nullable()->after('email');
            }
            if (! Schema::hasColumn('professionals', 'facebook')) {
                $table->string('facebook')->nullable()->after('instagram');
            }
            if (! Schema::hasColumn('professionals', 'years_experience')) {
                $table->unsignedSmallInteger('years_experience')->nullable()->after('facebook');
            }
        });

        $this->makeUserIdNullable();
    }

    public function down(): void
    {
        Schema::table('professionals', function (Blueprint $table) {
            $columns = array_filter(
                ['biography', 'instagram', 'facebook', 'years_experience'],
                fn (string $column) => Schema::hasColumn('professionals', $column),
            );

            if ($columns !== []) {
                $table->dropColumn($columns);
            }
        });
    }

    private function makeUserIdNullable(): void
    {
        $driver = Schema::getConnection()->getDriverName();

        if ($driver === 'sqlite') {
            // SQLite: recreate foreign key as nullable via table rebuild helper.
            Schema::table('professionals', function (Blueprint $table) {
                $table->unsignedBigInteger('user_id')->nullable()->change();
            });

            return;
        }

        Schema::table('professionals', function (Blueprint $table) {
            $table->dropForeign(['user_id']);
        });

        Schema::table('professionals', function (Blueprint $table) {
            $table->foreignId('user_id')->nullable()->change();
            $table->foreign('user_id')
                ->references('id')
                ->on('users')
                ->nullOnDelete();
        });
    }
};
