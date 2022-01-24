<template>
    <div class="col-lg-5" v-if="this.$route.name === 'resources'">
        <h1>{{title}}</h1>
        <ol class="breadcrumb">
            <li v-for="(item, index) in items" :key="index"
                :class="{'active': item.isActive}"
                class="breadcrumb-item">
                <router-link :to="{path: item.path}">{{item.label}}</router-link>
            </li>
        </ol>
    </div>
    <div class="col-lg-5" v-else>
        <h1>{{title}}</h1>
        <ol class="breadcrumb">
            <li v-for="(item, index) in items" :key="index"
                :class="{'active': item.isActive}"
                class="breadcrumb-item">
                <router-link :to="{path: item.path}">{{item.label}}</router-link>
            </li>
        </ol>
    </div>
</template>

<script>
    export default {
        name: "Breadcrumb",
        props: {
            title: String,
            items: Array
        }
    }
</script>
