<template>
  <div class="row">
    <div class="col-lg-12">
      <div class="ibox">
        <div class="ibox-content">
          <div class="spinner text-center" v-if="loadSpinner">
            <i class="fa fa-spinner fa-spin"></i>Loading
          </div>
          <div class="table-responsive" v-if="rows && allRows">
            <div
              class="dataTables_info"
              id="DataTables_Table_0_info"
              role="status"
              aria-live="polite"
            >
              <span v-if="rows.length === 0">
                Showing {{ filteredItems.length }} of {{ rows.length }} entries
              </span>
              <span v-else>
                Showing {{ filteredItems.length }} of
                {{ allRows.length }} entries
              </span>
            </div>
            <table
              :class="[
                'table',
                'table-striped',
                'table-hover',
                { 'table-fixed': fixedTable },
              ]"
            >
              <thead>
                <tr>
                  <th
                    v-for="(heading, index) in theadings"
                    :key="index"
                    @click="sortTable(heading)"
                    class="hover-heading"
                  >
                    <span class="column-header-content">
                      <p>{{ heading }}</p>
                      <span class="icon-sort" v-if="currentSortDir === 'asc'"
                        ><img
                          src="../../assets/img/auth_images/down-arrow-sort2.svg"
                          alt="logo image"
                      /></span>
                      <span class="icon-sort" v-else
                        ><img
                          class="icon-sort"
                          src="../../assets/img/auth_images/up-arrow-sort.svg"
                          alt="logo image"
                      /></span>
                    </span>
                  </th>
                  <th></th>
                </tr>
              </thead>
              <tbody>
                <tr
                  v-for="(item, index) in rows"
                  :key="index"
                  :data-id="item.id"
                >
                  <td
                    v-for="(field, columnIndex) in fields"
                    :key="columnIndex"
                    :class="[
                      'tcontent',
                      { id: field == '$.id' },
                      { 'break-td': breakWord },
                    ]"
                    :data-id="item.id"
                    @click="displayDetails"
                  >
                    {{ get_details(item, field) }}
                  </td>
                  <td class="action text-right">
                    <!--<a @click.prevent="!isActive">
                                        <i :class="['fa', 'fa-toggle-on', isActive, 'status', 'text-navy']"></i>
                                    </a>-->
                    <!-- display view user details if on users section -->
                    <a
                      v-if="$route.name == 'users' || $route.name == 'home'"
                      :data-id="item.id"
                      @click.prevent="displayDetails"
                      class="action-edit btn btn-white"
                    >
                      <i
                        :data-id="item.id"
                        @click.prevent="displayDetails"
                        class="fa fa-eye text-navy"
                      >
                      </i>
                    </a>
                    <a
                      v-else
                      :data-id="item.id"
                      @click.prevent="$emit('edit')"
                      class="action-edit btn btn-white"
                    >
                      <i
                        :data-id="item.id"
                        @click.prevent="$emit('edit')"
                        class="fa fa-pencil text-navy"
                      >
                      </i>
                    </a>
                    <!--  <a @click.prevent="$emit('edit')" class="action-edit btn btn-white "><i class="fa fa-trash text-navy"></i></a> -->
                  </td>
                </tr>
              </tbody>
              <tfoot>
                <tr>
                  <td colspan="8" class="footable-visible">
                    <div class="pagination float-right m-r-8 p-t-30">
                      <p class="footable-page">Rows per page:</p>
                      <div class="footable-page-arrow disabled">
                        <!-- <filter-table class="filta-data" :options = "options" @filterData = "displayRows($event)" /> -->
                        <v-select
                          class="inline"
                          transition="none"
                          v-model="selected"
                          @input="
                            rows.length > 0
                              ? setSelected(selected)
                              : setDefaultSelected()
                          "
                          :options="options"
                          :clearable="false"
                        >
                        </v-select>
                      </div>
                      <p class="footable-page">
                        <span v-if="rows.length > 0">
                          {{ pageCount }}-{{
                            itemsPerPage * pageNumber >= allRows.length
                              ? allRows.length
                              : itemsPerPage * pageNumber
                          }}
                          of {{ allRows.length }}
                        </span>
                        <span v-else>
                          {{ rows.length }}-{{ rows.length }} of
                          {{ rows.length }}
                        </span>
                      </p>
                      <div class="footable-page-actions">
                        <Button
                          @click="handlePageChange('previous')"
                          :disabled="this.pageNumber === 1 || rows.length === 0"
                          ><i class="fa fa-angle-left"></i
                        ></Button>

                        <Button
                          @click="handlePageChange('next')"
                          :disabled="
                            this.itemsPerPage * this.pageNumber >=
                              allRows.length || rows.length === 0
                          "
                          ><i class="fa fa-angle-right"></i
                        ></Button>
                      </div>
                    </div>
                  </td>
                </tr>
              </tfoot>
            </table>
            <div>
              <p v-if="rows.length === 0" class="no-data-message">
                <span v-if="$route.name !== 'home'">
                  Sorry, no {{ $route.name }} found
                </span>
                <span v-else> Sorry, no items found </span>
              </p>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
// import FilterTable from '@/components/shared/FilterTable'
import vSelect from "vue-select";
import { xpath_getter } from "@/utils/helpers";

export default {
  name: "Table",
  components: {
    vSelect,
    // FilterTable
  },

  data() {
    return {
      fixedTable: false,
      breakWord: false,
      loadSpinner: false,
      filteredItems: [],
      isActive: true,
      isReady: false,
      selected: 10,
      options: [10, 20, 30],
      oldRows: this.rows,
      itemsPerPage: 10,
      pageNumber: 1,
      currentPage: 0,
      pageCount: 1,
      maximumNumberOfRows: 10,
      isInitialLoad: true,
      currentRows: [],
      rowLength: 0,
      currentSort: "",
      currentSortDir: "asc",
    };
  },
  props: {
    theadings: Array,
    rows: Array,
    allRows: Array,
    fields: Array,
    displayDetails: Function,
    currentPath: String,
    edit: Function,
  },
  created() {
    // whenever rows changes, this function will run
    if (this.rows == null) {
      this.loadSpinner = true;
    }
    if (this.allRows != null) {
      this.rowLength = this.allRows.length;
    }
    if (this.$route.name == "clients") {
      this.fixedTable = true;
      this.breakWord = true;
    } else {
      this.fixedTable = false;
      this.breakWord = false;
    }
  },
  watch: {
    // whenever rows changes, this function will run
    rows: function (newRow) {
      this.loadSpinner = false;
      this.rowLength = this.allRows && this.allRows.length;
      this.filteredItems = newRow.slice(0, newRow.length);
      if (this.isInitialLoad) {
        this.rowLength = newRow.length;
        this.oldRows = JSON.parse(JSON.stringify(newRow));
        this.currentRows = JSON.parse(JSON.stringify(newRow));
        this.isInitialLoad = false;
      }
    },
  },
  computed: {
    isPreviousButtonDisabled() {
      return this.currentPage === 1;
    },
  },

  methods: {
    get_details(row, field_name) {
      return xpath_getter(row, field_name);
    },
    setPageNumber(value) {
      this.pageNumber = value;
      this.$emit("setPageNumber");
    },
    setDefaultSelected() {
      //set to default
      this.selected = 10;
    },
    setSelected(value) {
      this.selected = value;
      this.itemsPerPage = value;
      this.pageNumber = 1;
      this.pageCount = 1;
      this.$emit("filterData", value);
    },

    handlePageChange(value) {
      switch (value) {
        case "next":
          this.$emit("setPageNumber", 1);
          this.pageNumber += 1;
          this.pageCount += this.itemsPerPage;
          break;
        case "previous":
          this.$emit("setPageNumber", -1);
          this.pageNumber -= 1;
          this.pageCount -= this.itemsPerPage;
          break;
        default:
          this.currentPage = value;
      }
    },

    sortTable(value) {
      if (value === this.currentSort) {
        this.currentSortDir = this.currentSortDir === "asc" ? "desc" : "asc";
      } else {
        this.currentSort = value;
      }
      switch (value) {
        case "ID":
          this.rows.sort((rowOne, rowTwo) => {
            let modifier = 1;
            let firstItem = rowOne.id,
              secondItem = rowTwo.id;
            if (this.currentSortDir === "desc") modifier = -1;
            if (firstItem < secondItem) return -1 * modifier;
            if (firstItem > secondItem) return 1 * modifier;
            return 0;
          });
          break;
        case "Full Name":
          this.rows.sort((rowOne, rowTwo) => {
            let modifier = 1;
            let firstItem =
                rowOne.claims.name && rowOne.claims.name.toLowerCase(),
              secondItem =
                rowTwo.claims.name && rowTwo.claims.name.toLowerCase();
            if (this.currentSortDir === "desc") modifier = -1;
            if (firstItem < secondItem) return -1 * modifier;
            if (firstItem > secondItem) return 1 * modifier;
            return 0;
          });
          break;

        case "Username":
          this.rows.sort((rowOne, rowTwo) => {
            let modifier = 1;
            let firstItem =
                rowOne.preferredUsername &&
                rowOne.preferredUsername.toLowerCase(),
              secondItem =
                rowTwo.preferredUsername &&
                rowTwo.preferredUsername.toLowerCase();
            if (this.currentSortDir === "desc") modifier = -1;
            if (firstItem < secondItem) return -1 * modifier;
            if (firstItem > secondItem) return 1 * modifier;
            return 0;
          });
          break;
        case "Participant":
          this.rows.sort((rowOne, rowTwo) => {
            let modifier = 1;
            let firstItem =
                rowOne.claims.ParticipantName &&
                rowOne.claims.ParticipantName.toLowerCase(),
              secondItem =
                rowTwo.claims.ParticipantName &&
                rowTwo.claims.ParticipantName.toLowerCase();
            if (this.currentSortDir === "desc") modifier = -1;
            if (firstItem < secondItem) return -1 * modifier;
            if (firstItem > secondItem) return 1 * modifier;
            return 0;
          });
          break;
        case "Contact":
          this.rows.sort((rowOne, rowTwo) => {
            let modifier = 1;
            let firstItem = rowOne.telephone && rowOne.telephone.toLowerCase(),
              secondItem = rowTwo.telephone && rowTwo.telephone.toLowerCase();
            if (this.currentSortDir === "desc") modifier = -1;
            if (firstItem < secondItem) return -1 * modifier;
            if (firstItem > secondItem) return 1 * modifier;
            return 0;
          });
          break;
        case "Email":
          this.rows.sort((rowOne, rowTwo) => {
            let modifier = 1;
            let firstItem = rowOne.claims.email,
              secondItem = rowTwo.claims.email;
            if (this.currentSortDir === "desc") modifier = -1;
            if (firstItem < secondItem) return -1 * modifier;
            if (firstItem > secondItem) return 1 * modifier;
            return 0;
          });
          break;
        case "Role":
          this.rows.sort((rowOne, rowTwo) => {
            let modifier = 1;
            let firstItem = rowOne.roles[0],
              secondItem = rowTwo.roles[0];
            if (this.currentSortDir === "desc") modifier = -1;
            if (firstItem < secondItem) return -1 * modifier;
            if (firstItem > secondItem) return 1 * modifier;
            return 0;
          });
          break;

        case "Status":
          this.rows.sort((rowOne, rowTwo) => {
            let modifier = 1;
            let firstItem = rowOne.lockedOut,
              secondItem = rowTwo.lockedOut;
            if (this.currentSortDir === "desc") modifier = -1;
            if (firstItem < secondItem) return -1 * modifier;
            if (firstItem > secondItem) return 1 * modifier;
            return 0;
          });
          break;
        default:
          this.currentPage = value;
      }
    },
  },
};
</script>

<style scoped>
.fa-spin {
  margin-left: -12px;
  margin-right: 8px;
}
.spinner {
  font-size: 20px;
  padding-top: 200px;
  padding-bottom: 200px;
}
.table-fixed {
  table-layout: fixed;
}
.break-td {
  word-wrap: break-word;
}
</style>
