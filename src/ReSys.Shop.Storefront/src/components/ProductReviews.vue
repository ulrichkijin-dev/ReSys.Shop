<template>
  <div class="mt-16">
    <div class="flex items-center justify-between mb-8">
      <h2 class="text-2xl font-bold">Customer Reviews</h2>
      <Button label="Write a Review" icon="pi pi-pencil" @click="showReviewForm = true" v-if="isAuthenticated" />
    </div>

    <div v-if="loading" class="space-y-6">
      <Skeleton v-for="i in 3" :key="i" height="100px" />
    </div>

    <div v-else-if="reviews.length > 0" class="space-y-8">
      <div v-for="review in reviews" :key="review.id" class="border-b border-surface-200 dark:border-surface-700 pb-8">
        <div class="flex items-center justify-between mb-4">
          <div class="flex items-center gap-4">
            <Avatar :label="review.userName?.charAt(0)" shape="circle" size="large" />
            <div>
              <p class="font-bold">{{ review.userName }}</p>
              <div class="flex items-center gap-2">
                 <Rating :modelValue="review.rating" readonly :cancel="false" />
                 <span v-if="review.isVerifiedPurchase" class="text-xs bg-green-100 text-green-700 px-2 py-0.5 rounded-full font-bold">Verified Purchase</span>
              </div>
            </div>
          </div>
          <span class="text-sm text-surface-500">{{ new Date(review.createdAt).toLocaleDateString() }}</span>
        </div>
        <h3 class="font-bold mb-2">{{ review.title }}</h3>
        <p class="text-surface-600 dark:text-surface-400 mb-4">{{ review.comment }}</p>
        <div class="flex items-center gap-4">
          <span class="text-xs text-surface-500">Was this review helpful?</span>
          <Button icon="pi pi-thumbs-up" :label="String(review.helpfulCount)" text size="small" @click="vote(review.id, true)" />
          <Button icon="pi pi-thumbs-down" :label="String(review.notHelpfulCount)" text size="small" severity="secondary" @click="vote(review.id, false)" />
        </div>
      </div>
    </div>
    
    <div v-else class="text-center py-12 bg-surface-50 dark:bg-surface-900 rounded-xl">
      <p>No reviews yet. Be the first to review this product!</p>
    </div>

    <!-- Review Form Dialog -->
    <Dialog v-model:visible="showReviewForm" header="Write a Review" :style="{width: '450px'}">
       <div class="flex flex-col gap-4">
          <div class="flex flex-col gap-2">
            <label>Rating</label>
            <Rating v-model="form.rating" :cancel="false" />
          </div>
          <div class="flex flex-col gap-2">
            <label>Title</label>
            <InputText v-model="form.title" placeholder="Summarize your experience" />
          </div>
          <div class="flex flex-col gap-2">
            <label>Comment</label>
            <Textarea v-model="form.comment" rows="5" placeholder="What did you like or dislike?" />
          </div>
       </div>
       <template #footer>
          <Button label="Submit Review" @click="submitReview" :loading="submitting" />
       </template>
    </Dialog>
  </div>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue';
import { reviewService } from '@/services/review.service';
import { useAuthStore } from '@/stores/auth';
import Button from 'primevue/button';
import Rating from 'primevue/rating';
import Avatar from 'primevue/avatar';
import Skeleton from 'primevue/skeleton';
import Dialog from 'primevue/dialog';
import InputText from 'primevue/inputtext';
import Textarea from 'primevue/textarea';

const props = defineProps({
  productId: { type: String, required: true }
});

const authStore = useAuthStore();
const isAuthenticated = computed(() => authStore.isAuthenticated);

const reviews = ref([]);
const loading = ref(true);
const submitting = ref(false);
const showReviewForm = ref(false);

const form = ref({
  rating: 5,
  title: '',
  comment: ''
});

onMounted(fetchReviews);

async function fetchReviews() {
  loading.value = true;
  try {
    const response = await reviewService.getProductReviews(props.productId);
    reviews.value = response.data.items;
  } finally {
    loading.value = false;
  }
}

async function submitReview() {
  submitting.value = true;
  try {
    await reviewService.createReview(props.productId, form.value);
    showReviewForm.value = false;
    // Reset form
    form.value = { rating: 5, title: '', comment: '' };
    await fetchReviews();
  } finally {
    submitting.value = false;
  }
}

async function vote(id, helpful) {
  await reviewService.voteReview(id, helpful);
  // Refresh specifically this review or list
}
</script>
